using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pixelynx.Api.Models;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Api.Settings;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Models;
using Pixelynx.Core.Helpers;
using Stripe;
using Stripe.Issuing;
using AsyncHelper = Pixelynx.Core.Helpers.AsyncHelper;

namespace Pixelynx.Api.Controllers
{
    [Route("payment"), Authorize]
    public class PaymentController : Controller
    {
        private UnitOfWork unitOfWork;
        private IVaultService vaultService;
        private IDistributedCache cache;

        public PaymentController(UnitOfWork unitOfWork, IVaultService vaultService, IDistributedCache cache)
        {
            this.unitOfWork = unitOfWork;
            this.vaultService = vaultService;
            this.cache = cache;
        }

        [Route("token"), HttpGet, AllowAnonymous]
        public IActionResult GetStripeToken([FromServices]IOptions<StripeSettings> stripeSettings)
        {
            return Ok(stripeSettings.Value.PublishKey);
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
    }
}