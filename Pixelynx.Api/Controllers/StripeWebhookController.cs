using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pixelynx.Api.Models;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Models;
using Stripe;

namespace Pixelynx.Api.Controllers
{
    [Route("stripe")]
    public class StripeWebhookController : Controller
    {
        private UnitOfWork unitOfWork;
        private static string endpointSecret;
        private IVaultService vaultService;
        private IDbContextFactory dbContextFactory;
        
        public StripeWebhookController(UnitOfWork unitOfWork, IVaultService vaultService, IDbContextFactory dbContextFactory)
        {
            this.unitOfWork = unitOfWork;
            this.vaultService = vaultService;
            this.dbContextFactory = dbContextFactory;
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
                        using (var context = dbContextFactory.CreateReadWrite())
                        {
                            var cart = await context.Carts.Where(x => x.UserId == userId && x.Status == CartStatus.New)
                                .OrderByDescending(x => x.UpdatedDate).FirstOrDefaultAsync();

                            cart.Status = CartStatus.Complete;
                            context.Carts.Update(cart);
                            await context.SaveChangesAsync();
                        }
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