using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Pixelynx.Api.Extensions;
using Pixelynx.Api.Responses;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;

namespace Pixelynx.Api.Types
{
    public class GQLMutation
    {
        public async Task<IEnumerable<GQLAsset>> Download(IReadOnlyList<Guid> assetIds, 
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor,
            [Service] UserManager<UserEntity> userManager)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var email = contextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                UserEntity user = null;
                if (email != null)
                {
                    user = await userManager.FindByEmailAsync(email);
                }

                var purchasedAssets = await context.PurchasedAssets.ToListAsync();
                var assets = await context.Assets.Where(x => assetIds.Any(id => x.Id == id))
                    .ToGQLAsset()
                    .ToListAsync();

                bool notPurchased = assets.Any(x =>
                {
                    return x.Cost != 0 && purchasedAssets.All(asset => asset.AssetId != x.Id);
                });
                
                return notPurchased ? new List<GQLAsset>() : assets;
            }
        }

        [Authorize]
        public async Task<string> AddCredits(
            [Service] IPaymentService paymentService,
            [Service] IHttpContextAccessor contextAccessor,
            int amount)
        {
            if (!Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId) || amount < 20)
            {
                return null;
            }

            return await paymentService.CreatePaymentIntent(userId, amount, new Dictionary<string, string>
            {
                { "type", "CREDITS" },
                { "userId", userId.ToString() },
                { "amount", amount.ToString() },
            });
        }

        [Authorize]
        public async Task<PurchaseResponse> PurchaseAssets(
            [Service] IPaymentService paymentService,
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor,
            IReadOnlyCollection<Guid> assetIds, bool? useCredits)
        {
            if (!Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId))
            {
                return null;
            }
            var context = dbContextFactory.CreateReadWrite();
            var user = context.Users.First(x => x.Id == userId);

            var assetsToPurchase = new List<Guid>(assetIds);

            
            assetsToPurchase.RemoveAll(id => 
                context.PurchasedAssets.FirstOrDefault(x => x.UserId == userId && x.AssetId != id) == null
            );

            var total = assetsToPurchase.Sum(id => context.Assets.First(x => x.Id == id).Price);

            if (useCredits.Value == true)
            {
                var credits = user.Credits;
                if (credits < total) 
                {
                    return new PurchaseResponse
                    {
                        Succeeded = false,
                        Error = "Insufficient Credits"
                    };
                }
                else 
                {
                    user.Credits -= (int)total;
                    context.Users.Update(user);
                    assetIds.ForEach(async id => {
                        await context.PurchasedAssets.AddAsync(new PurchasedAssetEntity
                        {
                            AssetId = id,
                            UserId = userId,
                            TransactionId = $"cred_${total}_${Guid.NewGuid().ToString()}"
                        });
                    });

                    await context.SaveChangesAsync();

                    return new PurchaseResponse
                    {
                        Succeeded = true
                    };
                }
            }
            else 
            {
                return new PurchaseResponse 
                {
                    Succeeded = true,
                    Data = await paymentService.CreatePaymentIntent(userId, (int)total + 2, new Dictionary<string, string>
                    {
                        { "type", "ASSETS" },
                        { "userId", userId.ToString() },
                        { "assets", string.Join(',', assetIds) },
                    })
                };
            }
        }
    }
}