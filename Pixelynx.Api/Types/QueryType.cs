using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Settings;
using Pixelynx.Data.BlobStorage;

namespace Pixelynx.Api.Types
{
    public class Query 
    {
        public string Hello() => "world"; 

        public string Me([Service]IHttpContextAccessor context) => $"Hello, your Id is: {context.HttpContext.User.Identity.Name}";

        public async Task<List<Asset>> GetAssets(
            [Service]IBlobStorage blobStorage, 
            [Service]IOptions<AssetstoreSettings> assetstoreSettings, 
            string filter)
        {
            return (await blobStorage.ListObjects(assetstoreSettings.Value.BucketName))
                .GroupBy(x => x.Key.Split('/')[0])
                .Where(x => string.IsNullOrWhiteSpace(filter) || x.Key.Contains(filter))
                .Select(x => new Asset 
                {
                    Uri = x.FirstOrDefault(y => y.Key.EndsWith(".glb")).Uri,
                    ThumbnailUri = x.FirstOrDefault(y => !y.Key.EndsWith("glb") && y.Key != $"{x.Key}/").Uri,
                    Name = x.Key,
                }).ToList();
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