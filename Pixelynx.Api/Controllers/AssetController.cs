using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Requests;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;
using Microsoft.Extensions.Logging;

namespace Pixelynx.Api.Controllers
{
    [Route("asset")]
    public class AssetController : Controller
    {
        private IBlobStorage blobStorage;
        private UnitOfWork unitOfWork;
        private ILogger<AssetController> logger;

        public AssetController(IBlobStorage blobStorage, UnitOfWork unitOfWork, ILogger<AssetController> logger)
        {
            this.blobStorage = blobStorage;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsset([FromQuery] Guid id)
        {
            return Ok(await this.unitOfWork.AssetRepository.Value.GetAssetById(id));
        }

        [HttpPost, Route("uploadAsset")]
        public async Task<IActionResult> UploadAsset(
            [FromServices]IOptions<StorageSettings> storageSettings,
            [FromServices]UnitOfWork unitOfWork,
            [FromForm] UploadRequest request)
        {
            try 
            {
                var ms = new MemoryStream();
                request.Data.CopyTo(ms);
                Enum.TryParse<Core.AssetType>(request.Type, true, out var assetType);
                var asset = new Core.Asset(request.Name, assetType, ms.ToArray());

                if (!string.IsNullOrWhiteSpace(request.ParentId))
                {
                    asset.Parent = await unitOfWork.AssetRepository.Value.GetAssetById(Guid.Parse(request.ParentId));
                }

                await unitOfWork.AssetRepository.Value.CreateAsset(asset);
                await unitOfWork.SaveChanges();

                logger.Log(LogLevel.Information, $"Uploading Asset {request.Name} with ParentId: {request.ParentId}");

                return Ok(new { id = asset.Id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to create asset: {request.Name} of type {request.Type}");
                return BadRequest();
            }
        }
    }
}