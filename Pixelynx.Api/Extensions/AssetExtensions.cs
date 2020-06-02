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
            return ToGQLAsset(entity.AsQueryable());
        }
        public static IQueryable<GQLAsset> ToGQLAsset(this IQueryable<AssetEntity> entity)
        {
            return entity.Select(asset => new GQLAsset
            {
                Id = asset.Id,
                Name = asset.Name,
                Type = (Core.AssetType)asset.AssetType,
                Cost = (int)asset.Price,
                UploaderId = asset.UploaderId,
                ParentId = asset.Parent == null ? null : (Guid?)asset.Parent.Id,
                StorageId = asset.StorageId,
                StorageBuckets = new KeyValuePair<string, string>(asset.StorageBucket, asset.MediaStorageBucket),
                Children = asset.Children.Select(x => new GQLChildAsset
                {
                    Id = x.Id,
                    Type = (Core.AssetType)x.AssetType,
                    ParentId = x.ParentId.Value
                })
            });
        }
    }
}