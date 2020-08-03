using System;
using System.Collections.Generic;
using System.Linq;
using Pixelynx.Api.Types;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Api.Extensions
{
    public static class UserExtensions
    {
        public static IEnumerable<GQLUser> ToGQLUser(this IEnumerable<UserEntity> entity, Guid userId, IVaultService vaultService)
        {
            return entity.ToList().Select(user => user.ToGQLUser(userId, vaultService));
        }

        public static GQLUser ToGQLUser(this UserEntity user, Guid userId, IVaultService vaultService = null)
        {
            bool isAdmin = false;
            if (vaultService != null)
            {
                var admins = AsyncHelper.RunSync(vaultService.GetAuthSecrets).Admins;
                if (admins != null)
                {
                    isAdmin = admins.Split(";")
                        .Any(x => Guid.Parse(x) == userId);
                }
            }

            return new GQLUser
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = userId == user.Id ? user.Email : null,
                Credits = userId == user.Id ? (int?)user.Credits : null,
                IsAdmin = isAdmin,
                Me = user.Id == userId
            };
        }
    }
}