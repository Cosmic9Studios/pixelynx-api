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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;

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
    
        [HttpPost, Route("examine")]
        public IActionResult Examine(
            [FromServices]IDbContextFactory dbContextFactory, 
            IFormCollection request)
        {
            List<IFormFile> nonDuplicates = new List<IFormFile>();
            IQueryable<AssetEntity> assets = dbContextFactory.CreateRead().Assets;
            var fileData = new Dictionary<string, IFormFile>();

            foreach (var file in request.Files.Where(x => x.FileName.EndsWith("glb")))
            {
                var ms = new MemoryStream();
                file.CopyTo(ms);
                var byteArray = ms.ToArray();

                var fileHash = byteArray.GenerateHash();
                fileData[fileHash] = file;
                assets = assets.Where(x => x.FileHash == fileHash);
            }

            var fileNames = assets.Select(x => x.FileHash).ToList();
            return Ok(fileData.Where(x => !fileNames.Any(filename => filename == x.Key))
                .Select(x => Path.GetFileNameWithoutExtension(x.Value.FileName)));
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadAsset(
            [FromServices]UploadService uploadService,
            [FromServices]UserManager<UserEntity> userManager,
            [FromForm] UploadRequest request)
        {
            // Model has to be the first item in the form in order to act as the parent
            if (!request.Form.Files.Any() || (string.IsNullOrWhiteSpace(request.ParentId) && request.Form.Files[0].Name != "model"))
            {
                return BadRequest("Invalid form");
            }

            var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null) 
            {
                return BadRequest("Must be logged in to upload");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Must be logged in to upload"); 
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

            if(await uploadService.UploadAssets(user, assetData, Guid.TryParse(request.ParentId, out var parentId) ? (Guid?)parentId : null))
            {
                return Ok();
            }
            
            return BadRequest();
        }
    }
}