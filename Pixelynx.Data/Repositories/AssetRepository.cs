using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Core;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Data.Repositories
{
    public class AssetRepository
    {
        #region Fields.
        private IDbContextFactory dbContextFactory;
        #endregion

        #region Constructors.
        public AssetRepository(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }
        #endregion

        #region Query Methods.
        public async Task<long> GetAssetCost(Guid assetId)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var asset = await context.Assets.FirstAsync(x => x.Id == assetId);
                return asset.Price;
            }
        }
    
        public async Task<bool> IsOwned(Guid userId, Guid assetId)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var asset = await context.PurchasedAssets.FirstOrDefaultAsync(x => x.UserId == userId && x.AssetId == assetId);
                return asset != null;
            }
        }
        #endregion
    }
}