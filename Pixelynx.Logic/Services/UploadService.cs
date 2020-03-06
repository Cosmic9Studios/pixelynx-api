using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixelynx.Data.Models;
using Pixelynx.Logic.Model;

namespace Pixelynx.Logic.Services
{
    public class UploadService
    {
        private UnitOfWork unitOfWork;
        private ILogger<UploadService> logger;

        public UploadService(UnitOfWork unitOfWork, ILogger<UploadService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<bool> UploadAssets(List<AssetData> assetStreams, Core.Asset parent = null)
        {
            try 
            {
                foreach (var asset in assetStreams)
                {
                    var newAsset = new Core.Asset(asset.Metadata.Name, asset.Metadata.Type, asset.DataStream.ToArray());
                    newAsset.Thumbnail = new Core.Thumbnail(asset.ThumbnailStream.ToArray());
                    if (parent != null)
                    {
                        newAsset.Parent = parent;
                    }

                    await unitOfWork.AssetRepository.CreateAsset(newAsset);

                    // Model is always guaranteed to be first the list because form data comes in order (HTML SPEC)
                    if (asset.Metadata.Type == Core.AssetType.Model && parent == null)
                    {
                        parent = newAsset;
                    }
                    logger.Log(LogLevel.Information, $"Uploading Asset {asset.Metadata.Name} with ParentId: {parent?.Id}");
                }

                await unitOfWork.SaveChanges();
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