using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Core;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Settings;

namespace Pixelynx.Data.Repositories
{
    public class AssetRepository
    {
        #region Fields.
        private IDbContextFactory dbContextFactory;
        private IBlobStorage blobStorage;
        private string assetBucketName;
        private string mediaBucketName;
        #endregion

        #region Constructors.
        public AssetRepository(IDbContextFactory dbContextFactory, IBlobStorage blobStorage, StorageSettings storageSettings)
        {
            this.dbContextFactory = dbContextFactory;
            this.blobStorage = blobStorage;
            this.assetBucketName = storageSettings.AssetBucketName;
            this.mediaBucketName = storageSettings.MediaBucketName;
        }
        #endregion

        #region Mutation Methods.
        public async Task CreateAsset(Asset asset)
        {
            using (var context = dbContextFactory.CreateWrite())
            {
                var storageId = Guid.NewGuid();
                await blobStorage.UploadFileToBucket(assetBucketName, storageId.ToString(), "asset.glb", asset.RawData); 
                if (asset.Thumbnail != null)
                {
                    var name = $"thumbnail{Path.GetExtension(asset.Thumbnail.FileName)}";
                    await blobStorage.UploadFileToBucket(mediaBucketName, storageId.ToString(), name, asset.Thumbnail.RawData);
                }

                await context.Assets.AddAsync(new AssetEntity 
                {
                    Id = asset.Id, 
                    ParentId = asset.Parent?.Id, 
                    Name = asset.Name,
                    Description = asset.Description,
                    StorageBucket = assetBucketName,
                    StorageId = storageId,
                    MediaStorageBucket = mediaBucketName,
                    AssetType = (int)asset.Type, 
                    FileHash = asset.RawData.GenerateHash(),
                    Price = asset.Cost
                });

                await context.SaveChangesAsync();
            }
        }
        #endregion

        #region Query Methods.
        public async Task<IEnumerable<Asset>> GetAllAssets()
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var allAssets = await context.Assets.Include(x => x.Parent).ToListAsync();
                return await allAssets.Select(async x =>
                {
                    return await ToAsset(x);
                }).WhenAll();
            }
        }

        public async Task<IEnumerable<Asset>> FindAssets(string filter, string assetType, Guid? parentId, int? offset, int? limit)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                return await (await context.Assets.Include(x => x.Parent)
                    .Where(x => parentId == Guid.Empty || x.ParentId == parentId)
                    .Where(x => string.IsNullOrWhiteSpace(filter) || EF.Functions.ILike(x.Name, $"%{filter}%"))
                    .Where(x => string.IsNullOrWhiteSpace(assetType) || Convert.ToInt32(Enum.Parse<Core.AssetType>(assetType)) == x.AssetType)
                    .Skip(offset.HasValue ? offset.Value : 0)
                    .Take(limit.HasValue ? limit.Value : Int32.MaxValue)
                    .ToListAsync())
                    .Select(x => ToAsset(x, true))
                    .WhenAll();
            }
        }

        public async Task<Asset> GetAssetById(Guid id, bool signUrls = false)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                return await ToAsset(await context.Assets.Include(x => x.Parent).FirstAsync(x => x.Id == id), signUrls);
            }
        }

        public async Task<Asset> GetAssetByFileHash(string hash)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var entity = await context.Assets.Include(x => x.Parent).Where(x => x.FileHash == hash).FirstOrDefaultAsync();
                return entity == null ? null : await ToAsset(entity);
            }
        }

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

        #region Private Methods
        private async Task<Asset> ToAsset(AssetEntity entity, bool signUrls = false)
        {
            var asset = (await blobStorage.ListObjects(entity.StorageBucket, entity.StorageId.ToString(), signUrls))
                .First(x => x.Key.Contains("asset"));
            var thumbnail = (await blobStorage.ListObjects(entity.MediaStorageBucket, entity.StorageId.ToString()))
                .FirstOrDefault(x => x.Key.Contains("thumbnail"));

            var domainModel = new Asset(entity.Name, (AssetType) entity.AssetType, asset.Uri, entity.Id);
            domainModel.Cost = (int)entity.Price;
            if (thumbnail != null)
            {
                domainModel.Thumbnail = new Thumbnail(thumbnail.Uri);
            }

            if (entity.ParentId != null)
            {
                domainModel.Parent = await ToAsset(entity.Parent);
            }

            return domainModel;
        }
        #endregion
    }
}