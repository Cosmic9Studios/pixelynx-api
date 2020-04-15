using System;
using System.Collections.Generic;
using HotChocolate.Types;

namespace Pixelynx.Api.Types
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string ThumbnailUri { get; set; }
        public string Type { get; set; }
        public Guid? ParentId { get; set; }
        public int Cost { get; set; }
    }

    public class AssetType : ObjectType<Asset>
    {
        protected override void Configure(IObjectTypeDescriptor<Asset> descriptor)
        {
            descriptor.Field(f => f.Id.ToString()).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Name).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Uri).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.ThumbnailUri).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Type).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.ParentId).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Cost).Type<NonNullType<IntType>>();
        }
    }
}