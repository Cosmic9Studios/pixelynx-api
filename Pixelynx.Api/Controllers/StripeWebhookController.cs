using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.AspNetCore.Mvc;
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
                        var assets = paymentIntent.Metadata["assets"].Split(',').Select(Guid.Parse);
                        if (!assets.Any())
                        {
                            return BadRequest();
                        }

                        await unitOfWork.PaymentRepository.PurchaseAssets(userId, assets.ToList(), paymentIntent.Charges.Data[0].BalanceTransactionId);
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