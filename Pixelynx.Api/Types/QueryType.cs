using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Settings;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Models;

namespace Pixelynx.Api.Types
{
    public class Query
    {
        private UnitOfWork unitOfWork;

        public Query(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private readonly IEnumerable<string> modelTypes = new List<string> { ".glb", ".gltf" };

        public string Hello() => "world";

        public string Me([Service]IHttpContextAccessor context) => $"Hello, your Id is: {context.HttpContext.User.Identity.Name}";

        public async Task<List<Asset>> GetAssets(
            [Service]IBlobStorage blobStorage,
            [Service]IOptions<AssetstoreSettings> assetstoreSettings,
            string filter)
        {
            return (await unitOfWork.AssetRepository.Value.GetAllAssets())
                .Where(x => string.IsNullOrWhiteSpace(filter) || x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(x => 
                {
                    return new Asset
                    {
                        Uri = x.PreviewUri ?? x.Uri,
                        ThumbnailUri = x.ThumbnailUri,
                        Name = x.Name
                    };
                }).ToList();
        }

        public async Task<bool> UploadAsset(
            [Service]IBlobStorage blobStorage,
            [Service]IOptions<AssetstoreSettings> assetstoreSettings,
            string fileName,
            byte[] fileContent)
        {
            var extension = System.IO.Path.GetExtension(fileName);
            var name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var storageId = Guid.NewGuid();

            var result = await blobStorage.UploadFileToBucket(assetstoreSettings.Value.BucketName, storageId.ToString(), $"asset{extension}", fileContent);
            if (!result)
            {
                return false;
            }

            await unitOfWork.AssetRepository.Value.CreateAsset(new Core.Asset { Name = name }, assetstoreSettings.Value.BucketName, storageId);
            await unitOfWork.SaveChanges();

            return true;
        }
    }

    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(x => x.Hello()).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.Me(default)).Authorize();
            descriptor.Field(x => x.GetAssets(default, default, default));
        }
    }
}