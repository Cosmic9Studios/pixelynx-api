using System;
using System.Collections.Generic;
using System.Linq;
using Pixelynx.Api.Models;
using Pixelynx.Api.Types;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Models;
using Pixelynx.Logic.Services;

namespace Pixelynx.Api.Extensions
{
    public static class UserExtensions
    {
        public static IEnumerable<GQLUser> ToGQLUser(this IEnumerable<UserEntity> entity, Guid userId)
        {
            return entity.ToList().Select(user => user.ToGQLUser(userId));
        }

        public static GQLUser ToGQLUser(this UserEntity user, Guid userId)
        {
            return new GQLUser
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = userId == user.Id ? user.Email : null,
                Credits = userId == user.Id ? (int?)user.Credits : null,
                Me = user.Id == userId
            };
        }
    }
}