using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Http;
using Pixelynx.Api.Extensions;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Services;

namespace Pixelynx.Api.Middleware
{
    public class ToGQLAssetAttribute : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            bool single = false;
            descriptor.Use(next => async context =>
            {
                await next(context);
            
                if (context.Result == null) 
                {
                    return;
                }
                if(context.Result is IQueryable<object> query)
                {
                    var queryable = query.Cast<AssetEntity>();
                    context.Result = queryable.ToGQLAsset();
                }
                else
                {
                    var singleQuery = (AssetEntity)context.Result;
                    context.Result = singleQuery.ToGQLAsset();
                    single = true;
                }
            });

            if (single)
            {
                descriptor.Type<ObjectType<GQLAsset>>();
            }
            else 
            {
                descriptor.Type<ListType<ObjectType<GQLAsset>>>();
            }
        }
    }

    public class ToGQLUserAttribute : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            bool single = false;
            descriptor.Use(next => async context =>
            {
                await next(context);
                var contextAccessor = context.Service<IHttpContextAccessor>();
                var vaultService = context.Service<IVaultService>();
                var payoutService = context.Service<PayoutService>();
                Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var id);

                if(context.Result is IQueryable<object> query)
                {
                    var queryable = query.Cast<UserEntity>();
                    context.Result = queryable.ToGQLUser(id);
                }
                else
                {
                    var singleQuery = (UserEntity)context.Result;
                    context.Result = singleQuery.ToGQLUser(id);
                    single = true;
                }
            });
           
            if (single)
            {
                descriptor.Type<ObjectType<GQLUser>>();
            }
            else 
            {
                descriptor.Type<ListType<ObjectType<GQLUser>>>();
            }
        }
    }
}