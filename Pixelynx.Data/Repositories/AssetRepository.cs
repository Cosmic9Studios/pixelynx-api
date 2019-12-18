using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Core;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Entities;

namespace Pixelynx.Data.Repositories
{
    public class AssetRepository
    {
        private DbSet<AssetEntity> DbSet;
        private IBlobStorage blobStorage;

        public AssetRepository(PixelynxContext context, IBlobStorage blobStorage)
        {
            DbSet = context.Set<AssetEntity>();
            this.blobStorage = blobStorage;
        }

        public async Task CreateAsset(Asset asset, string storageBucket, Guid storageId)
        {
            await DbSet.AddAsync(new AssetEntity 
            {
                Id = asset.Id,
                Name = asset.Name,
                StorageBucket = storageBucket,
                StorageId = storageId
            });
        }

        public async Task<List<Asset>> GetAllAssets()
        {
            var allAssets = await DbSet.ToListAsync();
            return allAssets.Select(x =>
            {
                var blobs = AsyncHelper.RunSync(() => blobStorage.ListObjects(x.StorageBucket, x.StorageId.ToString()));
                var asset = blobs.First(x => x.Key.Contains("asset"));
                var thumbnail = blobs.FirstOrDefault(x => x.Key.Contains("thumbnail"));
                var watermark = blobs.FirstOrDefault(x => x.Key.Contains("watermark"));

                return new Asset
                {
                    Id = x.Id, 
                    Name = x.Name,
                    Uri = asset.Uri,
                    ThumbnailUri = thumbnail?.Uri,
                    PreviewUri = watermark?.Uri
                };
            }).ToList();
        }
    }
}