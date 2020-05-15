using System.Linq;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Extensions
{
    public static class UserExtensions
    {
        public static IQueryable<GQLUser> ToGQLUser(this IQueryable<UserEntity> entity)
        {
            return entity.Select(user => new GQLUser
            {
                Id = user.Id,
                Credits = user.Credits
            });
        }
    }
}