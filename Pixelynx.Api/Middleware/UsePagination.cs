using System.Linq;
using System.Reflection;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace Pixelynx.Api.Middleware
{
    public class UsePaginationAttribute : ObjectFieldDescriptorAttribute
    {
        public override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
        {
            descriptor.Use(next => async context =>
            {
                await next(context);
                var offset = context.Argument<int>("offset");
                var limit = context.Argument<int>("limit");

                if(context.Result is IQueryable<object> query)
                {
                    context.Result = query.Skip(offset).Take(limit > 0 ? limit : int.MaxValue);
                }
            });
        }
    }
}