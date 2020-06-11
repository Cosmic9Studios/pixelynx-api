using System;
using System.Linq;
using System.Reflection;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Middleware
{
    public class AssetFilterAttribute : ObjectFieldDescriptorAttribute
    {
        private int typeInt = 0;

        private IQueryable<AssetEntity> Filter(IQueryable<AssetEntity> query, GQLAssetFilter where) 
        {
            return query.Where(x => where.Id.HasValue ? x.Id == where.Id : true)
                .Where(x => where.ParentId.HasValue ? x.ParentId == where.ParentId : true)
                .Where(x => where.Type != null ? x.AssetType == typeInt : true)
                .Where(x => where.NameContains != null ? EF.Functions.ILike(x.Name, $"%{where.NameContains}%") : true);
        }

        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.Use(next => async context =>
            {
                await next(context);
                var where = context.Argument<GQLAssetFilter>("where");

                if(context.Result is IQueryable<AssetEntity> query)
                {
                    if (where != null)
                    {
                        if (where.Type != null && Enum.TryParse<Core.AssetType>(where.Type, true, out var type)) 
                        {
                            typeInt = (int)type;
                        }

                        if (where.OR != null)
                        {
                            foreach(var filter in where.OR)
                            {
                                query = Filter(query, filter);
                            }
                        }
                        else 
                        {
                            query = Filter(query, where);
                        }

                        context.Result = query;
                    }
                }
            });
        }
    }

    public class UserFilter : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.Use(next => async context =>
            {
                await next(context);
                var where = context.Argument<GQLUserFilter>("where");

                if(context.Result is IQueryable<UserEntity> query)
                {
                    if (where != null)
                    {
                        context.Result = query
                            .Where(x => where.Id.HasValue ? x.Id == where.Id : true)
                            .Where(x => where.UserName != null ? x.UserName == where.UserName : true);
                    }
                }
            });
        }
    }
}