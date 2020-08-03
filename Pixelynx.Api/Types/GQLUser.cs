using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Extensions;
using Pixelynx.Api.Middleware;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Stripe;

namespace Pixelynx.Api.Types
{
    public class GQLUserFilter
    {
        public Guid? Id { get; set; }
        public string UserName { get; set; }
    }

    public class GQLUser
    {
        public Guid Id { get; set; }
        public bool Me { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? Credits { get; set; }
        public float TotalBalance { get; set; }
        public float NextPayoutBalance { get; set; }
        
        public bool IsAdmin { get; set; }

        [ToGQLAsset]
        [UsePagination]
        [AssetFilter]
        public async Task<IQueryable<AssetEntity>> GetUploadedAssets(IResolverContext ctx, 
            [Service]IDbContextFactory dbContextFactory, GQLAssetFilter where, int? offset, int? limit) 
        {
            return (await ctx.GroupDataLoader<Guid, AssetEntity>("uploadedAssets", userIds => 
            {
                var context = dbContextFactory.CreateRead();
                var assets = context.Assets
                    .Include(x => x.Uploader)
                    .Where(x => userIds.Any(y => y == x.Uploader.Id));

                return Task.FromResult(assets.ToLookup(x => x.Uploader.Id));
            }).LoadAsync(ctx.Parent<GQLUser>().Id)).AsQueryable();
        }

        [Authorize]
        [ToGQLAsset]
        [UsePagination]
        [AssetFilter]
        public async Task<IQueryable<AssetEntity>> GetPurchasedAssets(IResolverContext ctx, 
            [Service]IDbContextFactory dbContextFactory, GQLAssetFilter where, int? offset, int? limit) 
        {
            return (await ctx.GroupDataLoader<Guid, AssetEntity>("purchasedAssets", userIds => 
            {
                var context = dbContextFactory.CreateRead();
                var assets = context.PurchasedAssets
                    .Include(x => x.Asset)
                    .ThenInclude(x => x.Uploader)
                    .Where(x => userIds.Any(y => y == x.UserId));
            
            return Task.FromResult(assets.ToLookup(x => x.UserId, x => x.Asset));
            }).LoadAsync(ctx.Parent<GQLUser>().Id)).AsQueryable();
        }
        
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