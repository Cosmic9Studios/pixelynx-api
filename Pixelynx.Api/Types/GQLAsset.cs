using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Api.Types
{
    public class GQLAsset 
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid UploaderId { get; set; }
        public string Name { get; set; }
        public Core.AssetType Type { get; set; }
        public int Cost { get; set; }

        [GraphQLIgnore]
        public Guid StorageId { get; set; }

        [GraphQLIgnore]
        public KeyValuePair<string, string> StorageBuckets { get; set; }
        
        public async Task<string> GetUri(IResolverContext ctx, [Service] IBlobStorage blobStorage)
        {
            return (await blobStorage.GetObject(StorageBuckets.Key, $"{StorageId.ToString()}/asset.glb", true)).Uri;
        }

        public async Task<string> GetThumbnailUri(IResolverContext ctx, [Service] IBlobStorage blobStorage)
        {
            return (await blobStorage.ListObjects(StorageBuckets.Value, StorageId.ToString(), false))
                .FirstOrDefault(x => x.Key.Contains("thumbnail"))?.Uri;
        }
        
        [UseFiltering]
        public async Task<GQLBuyer[]> GetBuyers(IResolverContext ctx, [Service]IDbContextFactory dbContextFactory) 
        {
            return await ctx.GroupDataLoader<Guid, GQLBuyer>("buyers", assetIds => 
            {
                var context = dbContextFactory.CreateRead();
                var assets = context.PurchasedAssets.Where(x => assetIds.Any(y => y == x.AssetId)).ToList();

                return Task.FromResult(assets.Select(x => new GQLBuyer(x.UserId, x.AssetId)).ToLookup(x => x.AssetId));
            }).LoadAsync(ctx.Parent<GQLAsset>().Id);
        }

    }
}