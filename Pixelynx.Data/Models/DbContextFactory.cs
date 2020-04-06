using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Data.Models
{
    public class DbContextFactory : IDbContextFactory
    {
        private string connectionString;
        private string pollutedConnectionString;
        private IVaultService vaultService;

        public DbContextFactory(string connectionString, IVaultService vaultService) 
        {
            this.vaultService = vaultService;
            this.connectionString = connectionString;
            var timer = new System.Threading.Timer((e) =>
            {
                var db = AsyncHelper.RunSync(() => vaultService.GetDbCredentials());
                pollutedConnectionString = connectionString
                    .Replace("{Db.UserName}", db.Key)
                    .Replace("{Db.Password}", db.Value);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        public PixelynxContext Create()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(pollutedConnectionString).Options;

            return new PixelynxContext(options);
        }
    }
}