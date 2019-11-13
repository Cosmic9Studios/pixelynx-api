using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Models;

namespace Pixelynx.Data
{
    public class PixelynxContext : IdentityDbContext<UserEntity, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        #region Constructors.
        /// <summary>
        /// Initializes a new instance of the <see cref="PixelynxContext" /> class.
        /// </summary>
        public PixelynxContext(DbContextOptions<PixelynxContext> options) : base(options)
        {
        }
        #endregion
    }
}