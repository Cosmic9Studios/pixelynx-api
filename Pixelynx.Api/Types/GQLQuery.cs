using System;
using System.Linq;
using System.Reflection;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Http;
using Pixelynx.Api.Extensions;
using Pixelynx.Api.Middleware;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Api.Types
{
    public class UseAssetFilter : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.UseFiltering<GQLAsset>();
        }
    }

    public class GQLQuery
    {
        [UsePagination]
        [UseAssetFilter]
        public IQueryable<GQLAsset> GetAssets([Service]IDbContextFactory context, int? offset, int? limit)
        {
            var myOffset = offset.HasValue ? offset.Value : 0;
            var myLimit = limit.HasValue ? limit.Value : Int32.MaxValue;
            return context.CreateRead().Assets
                .ToGQLAsset();
        }
        
        [UseFiltering]
        public IQueryable<GQLUser> GetUsers(
            [Service]IDbContextFactory context, 
            [Service] IHttpContextAccessor contextAccessor)
        {
            Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var id);
            return context.CreateRead().Users.ToGQLUser(id);
        }
            
        public string Hello() => "world";

        [Authorize]
        public GQLUser Me([Service]IDbContextFactory context, [Service]IHttpContextAccessor contextAccessor)
        {
            Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var id);
            return GetUsers(context, contextAccessor).FirstOrDefault(x => x.Id == id);
        }
    }
}