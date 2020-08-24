using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pixelynx.Logic.Models;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Models;
using Pixelynx.Logic.Interfaces;
using Stripe;

namespace Pixelynx.Api.Controllers
{
    [Route("stripe")]
    public class StripeWebhookController : Controller
    {
        private UnitOfWork unitOfWork;
        private static string endpointSecret;
        private readonly IVaultService vaultService;
        private readonly IDbContextFactory dbContextFactory;
        private readonly ICartService cartService;
        
        public StripeWebhookController(UnitOfWork unitOfWork, 
            IVaultService vaultService, 
            IDbContextFactory dbContextFactory,
            ICartService cartService)
        {
            this.unitOfWork = unitOfWork;
            this.vaultService = vaultService;
            this.dbContextFactory = dbContextFactory;
            this.cartService = cartService;
            if (string.IsNullOrEmpty(endpointSecret)) 
            {
                endpointSecret = AsyncHelper.RunSync(vaultService.GetAuthSecrets).StripeEndpointSecret;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var userId = Guid.Parse(paymentIntent.Metadata["userId"]);

                    var type = paymentIntent.Metadata["type"];
                    if (type == "ASSETS")
                    {
                        var assets = JsonConvert.DeserializeObject<List<AssetMetadata>>(paymentIntent.Metadata["assets"]);
                        if (!assets.Any())
                        {
                            return BadRequest();
                        }

                        await unitOfWork.PaymentRepository.PurchaseAssets(userId, assets.Select(x => x.Id).ToList(), paymentIntent.Charges.Data[0].BalanceTransactionId);
                        await cartService.UpdateCartStatus(userId, CartStatus.Complete);
                    }
                    else if (type == "CREDITS")
                    {
                        var amount = int.Parse(paymentIntent.Metadata["amount"]);
                        using (var context = dbContextFactory.CreateReadWrite())
                        {
                            var user = context.Users.FirstOrDefault(x => x.Id == userId);
                            user.Credits += amount;
                            context.Users.Update(user);
                            await context.SaveChangesAsync();
                        }
                    }
                }
                else if (stripeEvent.Type == Events.PaymentIntentProcessing) 
                {
                    // For testing
                    return Ok();
                }
                else
                {
                    // Unexpected event type
                    return BadRequest();
                }
                return Ok();
            }
            catch (StripeException)
            {
                endpointSecret = AsyncHelper.RunSync(() => vaultService.GetAuthSecrets()).StripeEndpointSecret;
                return BadRequest();
            }
        }
    }
}