using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Pixelynx.Api.Middleware;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Stripe;

namespace Pixelynx.Api.Types
{
    public class GQLQuery
    {
        public string Hello() => "world";

        [ToGQLAsset]
        [UsePagination]
        [AssetFilter]
        public IQueryable<AssetEntity> GetAssets(
            [Service]IDbContextFactory context, 
            GQLAssetFilter where, int? offset, int? limit) =>
            context.CreateRead().Assets
                .Include(x => x.Parent)
                .Include(x => x.Uploader)
                .Include(x => x.Children);
           
        [ToGQLUser]
        [UsePagination]
        [UserFilter]
        public IQueryable<UserEntity> GetUsers(
            [Service]IDbContextFactory context, GQLUserFilter where, int? offset, int? limit) => context.CreateRead().Users;
        
        [Authorize]
        [ToGQLUser]
        [UseFirstOrDefault]
        public IQueryable<UserEntity> Me([Service]IDbContextFactory context, [Service]IHttpContextAccessor contextAccessor)
        {
            Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var id);
            return context.CreateRead().Users.Where(x => x.Id == id);
        }

        [Authorize]
        [ToGQLAsset]
        public async Task<IQueryable<AssetEntity>> GetCartItems(
            [Service] IDbContextFactory context, 
            [Service]IHttpContextAccessor contextAccessor)
        {
            Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId);
            var dbContext = context.CreateRead();
            var cart = await dbContext.Carts.Where(x => x.UserId == userId && x.Status == CartStatus.New)
                .OrderByDescending(x => x.UpdatedDate).FirstOrDefaultAsync();
            return cart == null ? 
                new List<AssetEntity>().AsQueryable() : 
                dbContext.CartItems.Include(x => x.Asset).Where(x => x.CartId == cart.Id).Select(x => x.Asset);
        }
    }
}