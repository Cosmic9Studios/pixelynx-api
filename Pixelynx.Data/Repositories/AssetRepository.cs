using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Pixelynx.Core;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Entities;

namespace Pixelynx.Data.Repositories
{
    public class AssetRepository
    {
        #region Fields.
        private DbSet<AssetEntity> DbSet;
        private IBlobStorage blobStorage;
        private string bucketName;
        #endregion

        #region Constructors.
        public AssetRepository(PixelynxContext context, IBlobStorage blobStorage, string bucketName)
        {
            DbSet = context.Assets;
            this.blobStorage = blobStorage;
            this.bucketName = bucketName;
        }
        #endregion

        #region Mutation Methods.
        public async Task CreateAsset(Asset asset)
        {
            var storageId = Guid.NewGuid();
            await blobStorage.UploadFileToBucket(bucketName, storageId.ToString(), "asset.glb", asset.RawData); 
            if (asset.Thumbnail != null)
            {
                await blobStorage.UploadFileToBucket(bucketName, storageId.ToString(), "thumbnail.png", asset.Thumbnail.RawData);
            }

            await DbSet.AddAsync(new AssetEntity 
            {
                Id = asset.Id, 
                ParentId = asset.Parent?.Id, 
                Name = asset.Name,
                StorageBucket = bucketName,
                StorageId = storageId,
                AssetType = (int)asset.Type, 
                FileHash = asset.RawData.GenerateHash()
            });
        }
        #endregion

        #region Query Methods.
        public async Task<List<Asset>> GetAllAssets()
        {
            var allAssets = await DbSet.Include(x => x.Parent).ToListAsync();
            return allAssets.Select(x =>
            {
                return ToAsset(x);
            }).ToList();
        }

        public async Task<IEnumerable<Asset>> FindAssets(string filter, string assetType, Guid? parentId)
        {
            return (await DbSet.Include(x => x.Parent)
                .Where(x => parentId == Guid.Empty || x.ParentId == parentId)
                .Where(x => string.IsNullOrWhiteSpace(filter) || EF.Functions.ILike(x.Name, $"%{filter}%"))
                .Where(x => string.IsNullOrWhiteSpace(assetType) || Convert.ToInt32(Enum.Parse<Core.AssetType>(assetType)) == x.AssetType)
                .ToListAsync())
                .Select(ToAsset);
        }

        public async Task<IEnumerable<Asset>> GetAssetsById(Guid[] ids)
        {
            return (await DbSet.Include(x => x.Parent).Where(x => ids.Any(y => y == x.Id)).ToListAsync()).Select(x => ToAsset(x));
        }

        public async Task<Asset> GetAssetById(Guid id)
        {
            return ToAsset(await DbSet.Include(x => x.Parent).FirstAsync(x => x.Id == id));
        }

        public async Task<IEnumerable<Asset>> GetAssetsByType(AssetType assetType)
        {
            int type = (int)assetType;
            return (await DbSet.Include(x => x.Parent).Where(x => x.AssetType == type).ToListAsync()).Select(ToAsset);
        }

        public async Task<Asset> GetAssetByFileHash(string hash)
        {
            var entity = await DbSet.Include(x => x.Parent).Where(x => x.FileHash == hash).FirstOrDefaultAsync();
            return entity == null ? null : ToAsset(entity);
        }
        #endregion

        #region Private Methods
        private Asset ToAsset(AssetEntity entity)
        {
            var blobs = AsyncHelper.RunSync(() => blobStorage.ListObjects(entity.StorageBucket, entity.StorageId.ToString()));
            var asset = blobs.First(x => x.Key.Contains("asset"));
            var thumbnail = blobs.FirstOrDefault(x => x.Key.Contains("thumbnail"));

            var domainModel = new Asset(entity.Name, (AssetType) entity.AssetType, asset.Uri, entity.Id);
            if (thumbnail != null)
            {
                domainModel.Thumbnail = new Thumbnail(thumbnail.Uri);
            }

            if (entity.ParentId != null)
            {
                domainModel.Parent = ToAsset(entity.Parent);
            }

            return domainModel;
        }
        #endregion
    }
}