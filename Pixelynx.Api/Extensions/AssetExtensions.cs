using System;
using System.Collections.Generic;
using System.Linq;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Extensions
{
    public static class AssetExtensions
    {
        public static IEnumerable<GQLAsset> ToGQLAsset(this IEnumerable<AssetEntity> entity)
        {
            return entity.Where(x => x != null).Select(ToGQLAsset);
        }

        public static GQLAsset ToGQLAsset(this AssetEntity asset)
        {
            return new GQLAsset
            {
                Id = asset.Id,
                Name = asset.Name,
                Type = (Core.AssetType)asset.AssetType,
                Cost = (int)asset.Price,
                Uploader = asset.Uploader?.ToGQLUser(Guid.Empty),
                ParentId = asset.Parent == null ? null : (Guid?)asset.Parent.Id,
                StorageId = asset.StorageId,
                StorageBuckets = new KeyValuePair<string, string>(asset.StorageBucket, asset.MediaStorageBucket),
                Children = asset.Children?.AsQueryable()
            };
        }
    }
}