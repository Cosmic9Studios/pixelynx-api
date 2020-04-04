using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.AspNetCore.Mvc;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Data.Models;
using Stripe;

namespace Pixelynx.Api.Controllers
{
    [Route("payment")]
    public class PaymentController : Controller
    {
        private UnitOfWork unitOfWork;

        public PaymentController(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [Route("wallet/cards"), HttpGet]
        public async Task<IActionResult> GetCards()
        {
            var service = new PaymentMethodService();
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            var customerId = await unitOfWork.PaymentRepository.GetCustomerId(userId);

            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card",
            };
            var cards = service.List(
                options
            );

            return Ok(cards.Data.Select(x => new GetCardsResponse
            {
                Id = x.Id,
                Last4Digits = x.Card.Last4,
                Brand = x.Card.Brand, 
                ExpiryMonth = x.Card.ExpMonth,
                ExpiryYear = x.Card.ExpYear
            }));
        }

        [Route("wallet/card/{id}"), HttpDelete]
        public IActionResult DeleteCard(string id)
        {
            var service = new PaymentMethodService();
            service.Detach(id);
            return Ok();
        }

        [Route("wallet/card"), HttpPost]
        public async Task<IActionResult> AddCard()
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            var options = new SetupIntentCreateOptions{
                Customer = await unitOfWork.PaymentRepository.GetCustomerId(userId),
            };
            var service = new SetupIntentService();
            var intent = await service.CreateAsync(options);
            return Ok(intent.ClientSecret);
        }

        [Route("purchase")]
        public async Task<IActionResult> Purhase([FromBody] PurchaseAssetsRequest request)
        {
            var userId = HttpContext.User.Identity.Name;
            var total = request.Assets.Select(x => AsyncHelper.RunSync(() => unitOfWork.AssetRepository.GetAssetCost(x))).Sum();
            var options = new PaymentIntentCreateOptions
            {
                Customer = await unitOfWork.PaymentRepository.GetCustomerId(Guid.Parse(userId)),
                Amount = (long)total * 100,
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "assets", string.Join(',', request.Assets) },
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(paymentIntent.ClientSecret);
        }
    }
}