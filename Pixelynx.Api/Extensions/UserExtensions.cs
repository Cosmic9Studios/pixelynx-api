using System;
using System.Linq;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Extensions
{
    public static class UserExtensions
    {
        public static IQueryable<GQLUser> ToGQLUser(this IQueryable<UserEntity> entity, Guid userId)
        {
            return entity.Select(user => new GQLUser
            {
                Id = userId == user.Id ? (Guid?)user.Id : null,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = userId == user.Id ? user.Email : null,
                Credits = userId == user.Id ? (int?)user.Credits : null,
            });
        }
    }
}