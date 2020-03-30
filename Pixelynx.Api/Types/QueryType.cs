using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Arguments;
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
            descriptor.Field(x => x.GetAssets(default, default, default));
        }
    }
    
    public class Query
    {
        private UnitOfWork unitOfWork;

        public Query(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public string Hello() => "world";

        public string Me([Service]IHttpContextAccessor context) => $"Hello, your Id is: {context.HttpContext.User.Identity.Name}";

        public async Task<List<Asset>> GetAssets(
            [Service]IBlobStorage blobStorage,
            [Service]IOptions<StorageSettings> storageSettings,
            AssetArguments args)
        {
                var assets = await unitOfWork.AssetRepository.FindAssets(args.Filter, args.Type, Guid.TryParse(args.ParentId, out var guid) ? guid : Guid.Empty); 
                if (args.Random.Value) 
                {
                    Random rand = new Random();
                    int toSkip = rand.Next(0, assets.Count());
                    assets = assets.Skip(toSkip).Take(1);
                }

                return assets.Select(x =>
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