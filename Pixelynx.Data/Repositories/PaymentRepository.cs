using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Stripe;

namespace Pixelynx.Data.Repositories
{
    public class PaymentRepository
    {
        #region Fields.
        private IDbContextFactory dbContextFactory;
        #endregion

        #region Constructors.
        public PaymentRepository(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }
        #endregion

        #region Queries.
        public async Task<string> GetCustomerId(Guid userId)
        {
            using (var context = dbContextFactory.Create())
            {
                return (await context.PaymentDetails.FirstAsync(x => x.UserId == userId)).CustomerId;
            }
        }

        public async Task<string> GetDefaultPaymentId(Guid userId)
        {
            using (var context = dbContextFactory.Create())
            {
                return (await context.PaymentDetails.FirstAsync(x => x.UserId == userId)).DefaultPaymentMethodId;
            }
        }
        #endregion

        #region Mutations.
        public async Task SetDefaultPaymentId(Guid userId, string paymentId)
        {
            using (var context = dbContextFactory.Create())
            {
                var paymentDetails = await context.PaymentDetails.FirstAsync(x => x.UserId == userId);
                paymentDetails.DefaultPaymentMethodId = paymentId;
                context.Update(paymentDetails);
                await context.SaveChangesAsync();
            }
        }

        public async Task CreatePaymentAccount(Guid userId)
        {
            var options = new CustomerCreateOptions();
            var service = new CustomerService();
            var customer = service.Create(options);

            using (var context = dbContextFactory.Create())
            {
                await context.PaymentDetails.AddAsync(new PaymentDetailsEntity
                {
                    UserId = userId, 
                    CustomerId = customer.Id
                });

                await context.SaveChangesAsync();
            }
        }

        public async Task PurchaseAssets(Guid userId, List<Guid> assets, string transactionId)
        {
            using (var context = dbContextFactory.Create())
            {
                assets.ForEach(async asset => {
                    await context.PurchasedAssets.AddAsync(new PurchasedAssetEntity
                    {
                        AssetId = asset,
                        UserId = userId,
                        TransactionId = transactionId
                    });
                });
                
                await context.SaveChangesAsync();
            }
        }

        public async Task AddCredits(Guid userId, int credits) 
        {
            using (var context = dbContextFactory.Create())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) 
                {
                    throw new ArgumentException("User does not exist");
                }

                user.Credits += credits;
                context.Users.Update(user);

                await context.SaveChangesAsync();
            }
        }

        public async Task SpendCredits(Guid userId, int cost)
        {
            using (var context = dbContextFactory.Create())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) 
                {
                    throw new ArgumentException("User does not exist");
                }

                // Validate Transaction
                if (user.Credits < cost)
                {
                    throw new InvalidOperationException("Insufficient Funds");
                }

                user.Credits -= cost;
                context.Users.Update(user);

                await context.SaveChangesAsync();
            }
        }
        #endregion
    }
}