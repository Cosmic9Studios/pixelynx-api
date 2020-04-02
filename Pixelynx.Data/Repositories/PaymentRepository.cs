using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Enums;
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

        public async Task<string> GetCustomerId(Guid userId)
        {
            using (var context = dbContextFactory.Create())
            {
                return (await context.PaymentDetails.FirstAsync(x => x.UserId == userId)).CustomerId;
            }
        }

        public async Task CreatePaymentAccount(Guid userId)
        {
            var options = new CustomerCreateOptions();
            var service = new CustomerService();
            var customer = service.Create(options);

            using (var context = dbContextFactory.Create())
            {
                await context.PaymentDetails.AddAsync(new PaymentEntity
                {
                    UserId = userId, 
                    CustomerId = customer.Id
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

                await context.Transactions.AddAsync(new TransactionEntity
                {
                    UserId = userId,
                    Type = TransactionType.CREDIT,
                    Value = credits
                });

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

                await context.Transactions.AddAsync(new TransactionEntity
                {
                    UserId = userId,
                    Type = TransactionType.SPEND,
                    Value = cost
                });

                user.Credits -= cost;
                context.Users.Update(user);

                await context.SaveChangesAsync();
            }
        }
        #endregion
    }
}