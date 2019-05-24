using System.Collections.Generic;
using HotChocolate.Types;

namespace AssetStore.Api.Types
{
    public class Asset
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }

    public class AssetType : ObjectType<Asset>
    {
        protected override void Configure(IObjectTypeDescriptor<Asset> descriptor)
        {
            descriptor.Field(f => f.Name).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Data).Type<NonNullType<StringType>>();
        }
    }
}