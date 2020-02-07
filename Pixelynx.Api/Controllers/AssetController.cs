using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Requests;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;
using glTFLoader;

namespace Pixelynx.Api.Controllers
{
    [Route("asset")]
    public class AssetController : Controller
    {
        private IBlobStorage blobStorage;
        private UnitOfWork unitOfWork;

        public AssetController(IBlobStorage blobStorage, UnitOfWork unitOfWork)
        {
            this.blobStorage = blobStorage;
            this.unitOfWork = unitOfWork;
        }

        [HttpPost, Route("uploadAsset")]
        public async Task<IActionResult> UploadModel(
            [FromServices]IOptions<StorageSettings> storageSettings,
            [FromServices]UnitOfWork unitOfWork,
            [FromForm] UploadRequest request)
        {
            try 
            {
                var fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".gltf";
                var binaryFileName = Path.ChangeExtension(fileName, ".glb");

                var ms = new MemoryStream();
                request.Data.CopyTo(ms);
                System.IO.File.WriteAllBytes(fileName, ms.ToArray());

                Interface.Pack(fileName, binaryFileName);

                var fileBytes = System.IO.File.ReadAllBytes(binaryFileName);
            
                System.IO.File.Delete(fileName);
                System.IO.File.Delete(binaryFileName);

                Enum.TryParse<Core.AssetType>(request.Type, true, out var assetType);
                var asset = new Core.Asset(request.Name, assetType, fileBytes);

                if (!string.IsNullOrWhiteSpace(request.ParentId))
                {
                    asset.Parent = await unitOfWork.AssetRepository.Value.GetAssetById(Guid.Parse(request.ParentId));
                }

                await unitOfWork.AssetRepository.Value.CreateAsset(asset);
                await unitOfWork.SaveChanges();

                return Ok(new { id = asset.Id });
            }
            catch 
            {
                return BadRequest();
            }
        }
    }
}