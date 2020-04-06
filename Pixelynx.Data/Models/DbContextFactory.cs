using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Core.Helpers;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Data.Models
{
    public class DbContextFactory : IDbContextFactory
    {
        private string connectionString;
        private IVaultService vaultService;

        public DbContextFactory(string connectionString, IVaultService vaultService) 
        {
            this.vaultService = vaultService;
            this.connectionString = connectionString;
        }

        public PixelynxContext Create()
        {
            // Todo: Optimize so new password gets generate every x amount of times
            var db = AsyncHelper.RunSync(() => vaultService.GetDbCredentials());
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(connectionString
                    .Replace("{Db.UserName}", db.Key)
                    .Replace("{Db.Password}", db.Value)
                ).Options;

            return new PixelynxContext(options);
        }
    }
}