using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;
using Stripe;

public class PaymentService : IPaymentService
{
    private IDbContextFactory dbContextFactory;

    public PaymentService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<string> CreatePaymentIntent(Guid userId, int total, Dictionary<string, string> metadata)
    {
        using (var context = dbContextFactory.CreateRead())
        {
            var options = new PaymentIntentCreateOptions
            {
                Customer = (await context.PaymentDetails.FirstAsync(x => x.UserId == userId)).CustomerId,
                Amount = (long)total * 100,
                Currency = "usd",
                Metadata = metadata
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.ClientSecret;
        }
    }
}