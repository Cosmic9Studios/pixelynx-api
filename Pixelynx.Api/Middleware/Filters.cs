using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Middleware
{
    public class AssetFilterAttribute : ObjectFieldDescriptorAttribute
    {
        private int typeInt = 0;

        private Expression<Func<AssetEntity, bool>> Filter(GQLAssetFilter where)
        {
            if (where.Type != null && Enum.TryParse<Core.AssetType>(where.Type, true, out var type)) 
            {
                typeInt = (int)type;
            }

            var predicate = PredicateBuilder.New<AssetEntity>();
            if (where.Id.HasValue)
            {
                predicate = predicate.And(x => x.Id == where.Id);
            }
            if (where.ParentId.HasValue)
            {
                predicate = predicate.And(x => x.ParentId == where.ParentId);
            }
            if (where.Type != null)
            {
                predicate = predicate.And(x => x.AssetType == typeInt);
            }
            if (where.NameContains != null)
            {
                predicate = predicate.And(x => EF.Functions.ILike(x.Name, $"%{where.NameContains}%"));
            }

            return predicate;
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

                        var predicate = PredicateBuilder.New<AssetEntity>();
                        if (where.OR != null)
                        {
                            foreach(var filter in where.OR)
                            {
                                predicate = predicate.Or(Filter(filter));
                            }
                        }
                        else 
                        {
                            predicate = predicate.And(Filter(where));
                        }

                        context.Result = query.Where(predicate);
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