using Microsoft.EntityFrameworkCore;

namespace Pixelynx.Data.Models
{
    public class DbContextFactory
    {
        private string connectionString;

        public DbContextFactory(string connectionString) => this.connectionString = connectionString;

        public PixelynxContext Create()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new PixelynxContext(options);
        }
    }
}