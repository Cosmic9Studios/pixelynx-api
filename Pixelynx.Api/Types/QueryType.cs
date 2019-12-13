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

namespace Pixelynx.Api.Types
{
  public class Query
  {
    private readonly IEnumerable<string> modelTypes = new List<string> { ".glb", ".gltf" };

    public string Hello() => "world";

    public string Me([Service]IHttpContextAccessor context) => $"Hello, your Id is: {context.HttpContext.User.Identity.Name}";

    public async Task<List<Asset>> GetAssets(
        [Service]IBlobStorage blobStorage,
        [Service]IOptions<AssetstoreSettings> assetstoreSettings,
        string filter)
    {
      return (await blobStorage.ListObjects(assetstoreSettings.Value.BucketName))
          .GroupBy(x => x.Key.Split('/')[0])
          .Where(x => string.IsNullOrWhiteSpace(filter) || x.Key.Contains(filter, StringComparison.OrdinalIgnoreCase))
          .Select(x => new Asset
          {
            Uri = x.FirstOrDefault(y => modelTypes.Any(z => y.Key.EndsWith(z))).Uri,
            ThumbnailUri = x.FirstOrDefault(y => !modelTypes.Any(z => y.Key.EndsWith(z)) && y.Key != $"{x.Key}/").Uri,
            Name = x.Key,
          }).ToList();
    }

    public async Task<string> Upload(
        [Service]IBlobStorage blobStorage,
        [Service]IOptions<AssetstoreSettings> assetstoreSettings,
        string fileName,
        byte[] fileContent)
    {
      var response = await blobStorage.UploadFileToBucket(assetstoreSettings.Value.BucketName, fileName, fileContent);

      return response;
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