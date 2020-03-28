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
using Pixelynx.Data.Settings;

namespace Pixelynx.Api.Types
{
    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(x => x.Hello()).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.Me(default)).Authorize();
            descriptor.Field(x => x.GetAssets(default, default, default, default, default));
        }
    }
    
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
            [Service]IOptions<StorageSettings> storageSettings,
            string filter = null,
            string type = null,
            string parentId = null)
        {
                return (await unitOfWork.AssetRepository.FindAssets(filter, type, Guid.TryParse(parentId, out var guid) ? guid : Guid.Empty))
                .Select(x => 
                {
                    return new Asset
                    {
                        Id = x.Id,
                        Uri = x.Uri,
                        ThumbnailUri = x.Thumbnail?.Uri,
                        Name = x.Name,
                        Type = x.Type.ToString(),
                        ParentId = x.Parent?.Id
                    };
                }).ToList();
        }
    }
}