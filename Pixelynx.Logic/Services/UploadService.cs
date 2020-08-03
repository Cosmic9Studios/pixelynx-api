using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pixelynx.Core;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;
using Pixelynx.Logic.Model;

namespace Pixelynx.Logic.Services
{
    public class UploadService
    {
        private IDbContextFactory dbContextFactory;
        private ILogger<UploadService> logger;
        private StorageSettings storageSettings;
        private IBlobStorage blobStorage;

        public UploadService(IDbContextFactory dbContextFactory, 
            ILogger<UploadService> logger, 
            IOptions<StorageSettings> storageSettings,
            IBlobStorage blobStorage)
        {
            this.dbContextFactory = dbContextFactory;
            this.logger = logger;
            this.storageSettings = storageSettings.Value;
            this.blobStorage = blobStorage;
        }

        public async Task<bool> UploadAssets(UserEntity user, List<AssetData> assetStreams, Guid? parentId)
        {
            try 
            {
                var context = dbContextFactory.CreateWrite();
                foreach (var asset in assetStreams)
                {
                    var rawAssetData = asset.DataStream.ToArray();
                    var storageId = Guid.NewGuid();
                    await blobStorage.UploadFileToBucket(storageSettings.AssetBucketName, storageId.ToString(), "asset.glb", rawAssetData); 
                    if (!string.IsNullOrEmpty(asset.Metadata.Thumbnail))
                    {
                        var name = $"thumbnail{Path.GetExtension(asset.Metadata.Thumbnail)}";
                        await blobStorage.UploadFileToBucket(storageSettings.MediaBucketName, storageId.ToString(), name, asset.ThumbnailStream.ToArray());
                    }

                    var newAsset = new AssetEntity 
                    {
                        Id = Guid.NewGuid(), 
                        ParentId = parentId, 
                        UploaderId = user.Id,
                        Name = asset.Metadata.Name,
                        Description = asset.Metadata.Description,
                        StorageBucket = storageSettings.AssetBucketName,
                        StorageId = storageId,
                        MediaStorageBucket = storageSettings.MediaBucketName,
                        AssetType = (int)asset.Metadata.Type, 
                        Background = asset.Metadata.Background,
                        FileHash = rawAssetData.GenerateHash(),
                        Price = asset.Metadata.Price,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    await context.Assets.AddAsync(newAsset);

                    // Model is always guaranteed to be first the list because form data comes in order (HTML SPEC)
                    if (asset.Metadata.Type == AssetType.Model)
                    {
                        parentId = newAsset.Id;
                    }
                    logger.Log(LogLevel.Information, $"Uploading Asset {asset.Metadata.Name} with ParentId: {parentId}");
                }

                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, "Failed to upload assets");
                return false;
            }
        }
    }
}