using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Extensions;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;

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
    }
}