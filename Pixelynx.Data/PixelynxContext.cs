using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating( ModelBuilder builder ) 
        {
            base.OnModelCreating( builder );

            builder.Entity<UserEntity>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<UserToken>().ToTable("UserTokens");
            builder.Entity<UserLogin>().ToTable("UserLogins");
            builder.Entity<RoleClaim>().ToTable("RoleClaims");
            builder.Entity<UserClaim>().ToTable("UserClaims");

            builder.Entity<AssetEntity>().ToTable("Assets");
            builder.Entity<PaymentDetailsEntity>().ToTable("PaymentDetails");
            builder.Entity<PurchasedAssetEntity>().ToTable("PurchasedAssets");

            builder.Entity<AssetEntity>(entity =>
            {
                entity.Property(e => e.Price)
                .HasDefaultValue(0)
                .ValueGeneratedOnAddOrUpdate();
            });
        }

        public DbSet<AssetEntity> Assets { get; set; }  
        public DbSet<PaymentDetailsEntity> PaymentDetails { get; set; }
        public DbSet<PurchasedAssetEntity> PurchasedAssets { get; set; }
    }
}