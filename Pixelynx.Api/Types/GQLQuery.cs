using System;
using System.Linq;
using System.Reflection;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Http;
using Pixelynx.Api.Extensions;
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
        [UseAssetFilter]
        public IQueryable<GQLAsset> GetAssets([Service]IDbContextFactory context, int? offset, int? limit) =>
            context.CreateRead().Assets
                .Skip(offset.HasValue ? offset.Value : 0)
                .Take(limit.HasValue && limit.Value > 0 ? limit.Value : Int32.MaxValue)
                .ToGQLAsset();
        
        public string Hello() => "world";
        public string Me([Service]IHttpContextAccessor context) => $"Hello, your Id is: {context.HttpContext.User.Identity.Name}";
    }
}