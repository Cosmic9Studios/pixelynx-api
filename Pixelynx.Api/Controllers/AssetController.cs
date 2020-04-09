using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pixelynx.Api.Requests;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Models;
using Microsoft.Extensions.Logging;
using Pixelynx.Core.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Pixelynx.Logic.Services;
using Pixelynx.Logic.Model;
using Newtonsoft.Json;

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
            var asset = await this.unitOfWork.AssetRepository.GetAssetById(id, true);
            return Ok(asset);
        }

        [HttpPost, Route("examine")]
        public async Task<IActionResult> Examine(
            [FromServices]UnitOfWork unitOfWork, 
            IFormCollection request)
        {
            List<IFormFile> nonDuplicates = new List<IFormFile>();

            foreach (var file in request.Files.Where(x => x.FileName.EndsWith("glb")))
            {
                var ms = new MemoryStream();
                file.CopyTo(ms);
                var byteArray = ms.ToArray();

                var fileHash = byteArray.GenerateHash();
                var asset = await unitOfWork.AssetRepository.GetAssetByFileHash(fileHash);
                if (asset == null)
                {
                    nonDuplicates.Add(file);
                }
            }
        
            return Ok(nonDuplicates.Select(x => Path.GetFileNameWithoutExtension(x.FileName)));
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadAsset(
            [FromServices]UploadService uploadService,
            [FromForm] UploadRequest request)
        {
            // Model has to be the first item in the form in order to act as the parent
            if (!request.Form.Files.Any() || (string.IsNullOrWhiteSpace(request.ParentId) && request.Form.Files[0].Name != "model"))
            {
                return BadRequest("Invalid form");
            }

            Core.Asset parent = null;
            if (!string.IsNullOrWhiteSpace(request.ParentId))
            {
                parent = await unitOfWork.AssetRepository.GetAssetById(Guid.Parse(request.ParentId));
            }

            var formFiles = request.Form.Files.GroupBy(x => x.Name);
            List<AssetData> assetData = new List<AssetData>();
        
            foreach (var group in formFiles) 
            {
                var data = group.ElementAt(0);
                var thumbnail = group.ElementAt(1);
                request.Form.TryGetValue(group.Key, out var meta);

                var dataStream = new MemoryStream();
                var thumbStream = new MemoryStream();

                await data.CopyToAsync(dataStream);
                await thumbnail.CopyToAsync(thumbStream);
                var metadata = JsonConvert.DeserializeObject<AssetMetadata>(meta.First());

                assetData.Add(new AssetData
                {
                    DataStream = dataStream, 
                    ThumbnailStream = thumbStream, 
                    Metadata = metadata
                });
            }

            if(await uploadService.UploadAssets(assetData, parent))
            {
                return Ok();
            }
            
            return BadRequest();
        }
    }
}