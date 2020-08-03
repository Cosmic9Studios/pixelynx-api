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
        public static IEnumerable<GQLUser> ToGQLUser(this IEnumerable<UserEntity> entity, Guid userId, PayoutService payoutService, IVaultService vaultService)
        {
            return entity.ToList().Select(user => user.ToGQLUser(userId, payoutService, vaultService));
        }

        public static GQLUser ToGQLUser(this UserEntity user, Guid userId, PayoutService payoutService = null, IVaultService vaultService = null)
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
            
            var payoutBalance = new UserBalance();
            if (payoutService != null)
            {
                payoutBalance = AsyncHelper.RunSync(() => payoutService.GetUserBalance(user.Id));
            }

            return new GQLUser
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = userId == user.Id ? user.Email : null,
                Credits = userId == user.Id ? (int?)user.Credits : null,
                TotalBalance = userId == user.Id ? payoutBalance.TotalBalance : 0,
                NextPayoutBalance = userId == user.Id ? payoutBalance.NextPayoutBalance : 0,
                IsAdmin = isAdmin,
                Me = user.Id == userId
            };
        }
    }
}