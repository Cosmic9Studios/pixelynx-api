using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Settings;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Models;

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

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadMesh(
            [FromServices]IOptions<AssetstoreSettings> assetstoreSettings,
            [FromForm] UploadMeshRequest request
        )
        {
            var ms = new MemoryStream();
            request.Asset.CopyTo(ms);
            var contents = ms.ToArray();

            var extension = System.IO.Path.GetExtension(request.Asset.FileName);
            var name = System.IO.Path.GetFileNameWithoutExtension(request.Asset.FileName);
            var storageId = Guid.NewGuid();

            var result = await blobStorage.UploadFileToBucket(assetstoreSettings.Value.BucketName, storageId.ToString(), $"asset{extension}", contents);
            if (!result)
            {
                return BadRequest();
            }

            await unitOfWork.AssetRepository.Value.CreateAsset(new Core.Asset { Name = name }, assetstoreSettings.Value.BucketName, storageId);
            await unitOfWork.SaveChanges();

            return Ok();
        }
    }
}