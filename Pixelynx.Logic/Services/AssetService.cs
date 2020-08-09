using System;
using System.Linq;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;

namespace Pixelynx.Logic.Services
{
    public class AssetService : IAssetService
    {
        private IDbContextFactory dbContextFactory;

        public AssetService(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public bool IsOwned(Guid userId, Guid assetId)
        {
            using (var dbContext = dbContextFactory.CreateRead())
            {
                return dbContext.PurchasedAssets.FirstOrDefault(x => x.UserId == userId && x.AssetId == assetId) != null ||
                    dbContext.Assets.First(x => x.Id == assetId).UploaderId == userId;
            }
        }
    }
}