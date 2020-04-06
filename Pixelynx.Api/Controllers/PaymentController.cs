using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Data.Models;
using Stripe;

namespace Pixelynx.Api.Controllers
{
    [Route("payment"), Authorize]
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

        [Route("wallet/cards/default"), HttpPost]
        public async Task<IActionResult> SetDefaultCard([FromBody] SetDefaultCardRequest request)
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            await unitOfWork.PaymentRepository.SetDefaultPaymentId(userId, request.PaymentMethodId);
            return Ok();
        }

        [Route("wallet/cards/default"), HttpGet]
        public async Task<IActionResult> GetDefaultCard()
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            return Ok(await unitOfWork.PaymentRepository.GetDefaultPaymentId(userId));
        } 

        [Route("purchase")]
        public async Task<IActionResult> Purhase([FromBody] PurchaseAssetsRequest request)
        {
            var userId = HttpContext.User.Identity.Name;
            var assets = request.Assets.Where(x => AsyncHelper.RunSync(() => unitOfWork.AssetRepository.IsOwned(Guid.Parse(userId), x)) == false);
            var total = assets.Select(x => AsyncHelper.RunSync(() => unitOfWork.AssetRepository.GetAssetCost(x))).Sum();
            
            if (total == 0)
            {
                if (assets.Any())
                {
                    await unitOfWork.PaymentRepository.PurchaseAssets(Guid.Parse(userId), assets.ToList(), "free");
                }
                return Ok("free");
            }
            
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