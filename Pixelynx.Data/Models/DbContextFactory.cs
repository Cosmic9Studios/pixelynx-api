using System;
using System.Diagnostics;
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
        private string adminConnectionString;
        private string readConnectionString;
        private string writeConnectionString;
        private string readWriteConnectionString;
        private string sessionConnectionString;

        public string AdminConnectionString => adminConnectionString;
        public string SessionConnectionString => sessionConnectionString;

        private Stopwatch adminWatch = null;
        private Stopwatch readWatch = null;
        private Stopwatch writeWatch = null;
        private Stopwatch readWriteWatch = null;

        private IVaultService vaultService;
        private ILoggerFactory loggerFactory;

        public DbContextFactory(string connectionString, IVaultService vaultService, ILoggerFactory loggerFactory) 
        {
            this.vaultService = vaultService;
            this.connectionString = connectionString;
            this.loggerFactory = loggerFactory;
        }

        public PixelynxContext CreateAdmin()
        {
            if (adminWatch == null || adminWatch.Elapsed > TimeSpan.FromSeconds(10)) 
            {
                adminConnectionString = GetConnectionString(VaultRole.ADMIN);
                adminWatch = Stopwatch.StartNew();
            }

            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(adminConnectionString).Options;

            return new PixelynxContext(options, loggerFactory);
        }

        public PixelynxContext CreateRead()
        {
            if (readWatch == null || readWatch.Elapsed > TimeSpan.FromMinutes(10)) 
            {
                readConnectionString = GetConnectionString(VaultRole.READ);
                readWatch = Stopwatch.StartNew();
            }

            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(readConnectionString).Options;

            return new PixelynxContext(options, loggerFactory);
        }

        public PixelynxContext CreateReadWrite()
        {
            if (readWriteWatch == null || readWriteWatch.Elapsed > TimeSpan.FromMinutes(10)) 
            {
                readWriteConnectionString = GetConnectionString(VaultRole.READ_WRITE);
                readWriteWatch = Stopwatch.StartNew();
            }

            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(readWriteConnectionString).Options;

            return new PixelynxContext(options, loggerFactory);
        }

        public PixelynxContext CreateWrite()
        {
            if (writeWatch == null || writeWatch.Elapsed > TimeSpan.FromMinutes(10)) 
            {
                writeConnectionString = GetConnectionString(VaultRole.WRITE);
                writeWatch = Stopwatch.StartNew();
            }

            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(writeConnectionString).Options;

            return new PixelynxContext(options, loggerFactory);
        }
        
        public PixelynxContext CreateSession()
        {
            if (string.IsNullOrWhiteSpace(sessionConnectionString))
            {
                sessionConnectionString = GetConnectionString(VaultRole.SESSION);
            }
   
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(sessionConnectionString).Options;

            return new PixelynxContext(options, loggerFactory);
        }

        private string GetConnectionString(VaultRole role)
        {
            var db = AsyncHelper.RunSync(() => vaultService.GetDbCredentials(role));
            return connectionString
                .Replace("{Db.UserName}", db.Key)
                .Replace("{Db.Password}", db.Value);
        }
    }
}