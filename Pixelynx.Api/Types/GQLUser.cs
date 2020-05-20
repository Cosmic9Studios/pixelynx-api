using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Data.Interfaces;
using Stripe;

namespace Pixelynx.Api.Types
{
    public class GQLUser
    {
        public Guid? Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? Credits { get; set; }
        
        [Authorize]
        public async Task<IEnumerable<GQLCard>> GetCards(
            [Service] IHttpContextAccessor contextAccessor,
            [Service] IDbContextFactory dbContextFactory)
        {
            var service = new PaymentMethodService();
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            
            if (userId != Id) {
                return null;
            }

            string customerId;
            string defaultPaymentId;
            using (var context = dbContextFactory.CreateRead())
            {
                var paymentDetails =  await context.PaymentDetails.FirstAsync(x => x.UserId == userId);
                customerId = paymentDetails.CustomerId;
                defaultPaymentId = paymentDetails.DefaultPaymentMethodId;
            }

            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card",
            };
            var cards = service.List(
                options
            );

            return cards.Data.Select(x => new GQLCard
            {
                Id = x.Id,
                Last4Digits = x.Card.Last4,
                Brand = x.Card.Brand, 
                ExpiryMonth = x.Card.ExpMonth,
                ExpiryYear = x.Card.ExpYear,
                IsDefault = x.Id == defaultPaymentId
            });
        }
    }
}