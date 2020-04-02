using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Route("cards"), HttpGet]
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

        [Route("wallet"), HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            var options = new SetupIntentCreateOptions{
                Customer = await unitOfWork.PaymentRepository.GetCustomerId(userId),
            };
            var service = new SetupIntentService();
            var intent = service.Create(options);
            return Ok(intent.ClientSecret);
        }
    }
}