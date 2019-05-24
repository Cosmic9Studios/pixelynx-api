using System.Collections.Generic;
using HotChocolate.Types;

namespace AssetStore.Api.Types
{
    public class Query
    {
        public string Hello() => "world"; 

        public List<Asset> Assets()
        {
            return new List<Asset> 
            { 
                new Asset() { Name = "Foo", Data = "My Data"},
                new Asset() { Name = "Foo 2", Data = "My Data 2"}
            };
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