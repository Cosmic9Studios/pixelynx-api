using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Storage.V1;
using HotChocolate.Types;

namespace AssetStore.Api.Types
{
    public class Query
    {
        public string Hello() => "world"; 

        public List<Asset> Assets()
        {
            var client = StorageClient.Create();
            var assets = client.ListObjects("c9s-asset-storage")
                    .GroupBy(x => x.Name.Split('/')[0])
                    .Select(x => new Asset 
                    {
                        Name = x.Key,
                        Uri = x.FirstOrDefault(y => y.Name.EndsWith(".glb")).MediaLink,
                        ThumbnailUri = x.FirstOrDefault(y => !y.Name.EndsWith("glb") && y.Name != $"{x.Key}/").MediaLink
                    }).ToList();

            return assets;
        }
    }

    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(f => f.Hello()).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Assets()).Type<ListType<AssetType>>();
        }
    }
}