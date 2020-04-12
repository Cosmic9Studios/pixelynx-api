using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Models;

namespace Pixelynx.Data
{
    public class PixelynxContext : IdentityDbContext<UserEntity, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        #region Fields. 
        private ILoggerFactory loggerFactory;
        #endregion

        #region Constructors.
        /// <summary>
        /// Initializes a new instance of the <see cref="PixelynxContext" /> class.
        /// </summary>
        public PixelynxContext(DbContextOptions<PixelynxContext> options, ILoggerFactory loggerFactory) : base(options)
        {
            this.loggerFactory =  loggerFactory;
        }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)  
        {
            // Allow null if you are using an IDesignTimeDbContextFactory
            if (loggerFactory != null)
            { 
                if (Debugger.IsAttached)
                {
                    // Probably shouldn't log sql statements in production
                    optionsBuilder.UseLoggerFactory(this.loggerFactory); 
                }
            }
        } 

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
        }

        public DbSet<AssetEntity> Assets { get; set; }  
        public DbSet<PaymentDetailsEntity> PaymentDetails { get; set; }
        public DbSet<PurchasedAssetEntity> PurchasedAssets { get; set; }
    }
}