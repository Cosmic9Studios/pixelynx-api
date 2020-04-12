using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Data.Models
{
    public class DbContextFactory : IDbContextFactory
    {
        private string connectionString;
        private string pollutedConnectionString;
        private IVaultService vaultService;
        private ILoggerFactory loggerFactory;

        public DbContextFactory(string connectionString, IVaultService vaultService, ILoggerFactory loggerFactory) 
        {
            this.vaultService = vaultService;
            this.connectionString = connectionString;
            this.pollutedConnectionString = GetConnectionString();

            var timer = new System.Threading.Timer((e) =>
            {
                pollutedConnectionString = GetConnectionString();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            this.loggerFactory = loggerFactory;
        }

        public PixelynxContext Create()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(pollutedConnectionString).Options;

            return new PixelynxContext(options, loggerFactory);
        }

        private string GetConnectionString()
        {
            var db = AsyncHelper.RunSync(() => vaultService.GetDbCredentials());
            return connectionString
                .Replace("{Db.UserName}", db.Key)
                .Replace("{Db.Password}", db.Value);
        }
    }
}