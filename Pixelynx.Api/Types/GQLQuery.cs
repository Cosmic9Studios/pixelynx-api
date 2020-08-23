using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Middleware;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;

namespace Pixelynx.Api.Types
{
    public class GQLQuery
    {
        public string Hello() => "world";

        [ToGQLAsset]
        [UsePagination]
        [AssetFilter]
        public IQueryable<AssetEntity> GetAssets(
            [Service] IDbContextFactory context,
            GQLAssetFilter where, int? offset, int? limit) =>
            context.CreateRead().Assets
                .Include(x => x.Parent)
                .ThenInclude(x => x.Uploader)
                .Include(x => x.Uploader)
                .Include(x => x.Children)
                .ThenInclude(x => x.Uploader);
           
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
            [Service] ICartService cartService,
            [Service]IHttpContextAccessor contextAccessor)
        {
            Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId);

            var cartItems = await cartService.GetCartItems(userId);
            return cartItems.Select(x => x.Asset).AsQueryable();
        }
    }
}