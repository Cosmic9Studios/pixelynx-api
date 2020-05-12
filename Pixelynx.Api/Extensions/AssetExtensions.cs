using System;
using System.Collections.Generic;
using System.Linq;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Extensions
{
     public static class AssetEntityExtensions
    {
        public static IQueryable<GQLAsset> ToGQLAsset(this IQueryable<AssetEntity> entity)
        {
            return entity.Select(asset => new GQLAsset
            {
                Id = asset.Id,
                Name = asset.Name,
                Type = ((Core.AssetType)asset.AssetType),
                Cost = (int)asset.Price,
                UploaderId = asset.UploaderId,
                ParentId = asset.Parent == null ? Guid.Empty : asset.Parent.Id,
                StorageId = asset.StorageId,
                StorageBuckets = new KeyValuePair<string, string>(asset.StorageBucket, asset.MediaStorageBucket)
            });
        }
    }
}