using System.Collections.Generic;
using HotChocolate.Types;

namespace Pixelynx.Api.Types
{
    public class Asset
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string ThumbnailUri { get; set; }
    }

    public class AssetType : ObjectType<Asset>
    {
        protected override void Configure(IObjectTypeDescriptor<Asset> descriptor)
        {
            descriptor.Field(f => f.Name).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Uri).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.ThumbnailUri).Type<NonNullType<StringType>>();
        }
    }
}