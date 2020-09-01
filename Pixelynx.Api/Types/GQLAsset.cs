using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Middleware;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Api.Types
{
    public class GQLAssetFilter
    {
        public Guid? Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Type { get; set; }
        [GraphQLName("name_contains")]
        public string NameContains { get; set; }

        [GraphQLName("OR")]
        public List<GQLAssetFilter> OR { get; set; }
    }

    public class GQLAsset
    {
        public Guid Id { get; set; }
        public GQLAsset Parent { get; set; }
        public Guid? ParentId { get; set; }
        public Guid UploaderId { get; set;}
        public GQLUser Uploader { get; set; }
        public string Name { get; set; }
        public Core.AssetType Type { get; set; }
        public AssetLicense License { get; set; }
        public int Cost { get; set; }
        public int Background { get; set; }

        [ToGQLAsset]
        [AssetFilter]
        public IQueryable<AssetEntity> GetChildren(GQLAssetFilter where) => Children;

        [GraphQLIgnore]
        public IQueryable<AssetEntity> Children { get; set; }

        [GraphQLIgnore]
        public Guid StorageId { get; set; }

        [GraphQLIgnore]
        public KeyValuePair<string, string> StorageBuckets { get; set; }

        public async Task<string> GetUri(IResolverContext ctx, [Service] IBlobStorage blobStorage)
        {
            return (await blobStorage.GetObject(StorageBuckets.Key, $"{StorageId.ToString()}/asset.glb")).Uri;
        }

        public async Task<string> GetThumbnailUri(IResolverContext ctx, [Service] IBlobStorage blobStorage)
        {
            return (await blobStorage.ListObjects(StorageBuckets
            .Value, StorageId.ToString()))
                .FirstOrDefault(x => x.Key.Contains("thumbnail"))?.Uri;
        }

        [ToGQLUser]
        [UserFilter]
        public async Task<IQueryable<UserEntity>> GetBuyers(IResolverContext ctx,
            [Service] IDbContextFactory dbContextFactory, GQLUserFilter where)
        {
            return (await ctx.GroupDataLoader<Guid, UserEntity>("buyers", assetIds =>
            {
                var context = dbContextFactory.CreateRead();
                var assets = context
                    .PurchasedAssets.Include(x => x.User).Where(x => assetIds.Any(y => y == x.AssetId));

                return Task.FromResult(assets.ToLookup(x => x.AssetId, x => x.User));
            }).LoadAsync(ctx.Parent<GQLAsset>().Id)).AsQueryable();
        }
    }
}