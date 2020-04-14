using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Data;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Tests.Factories
{
    public class SqliteDbContextFactory : IDbContextFactory
    {
        private SqliteConnection connection;
        public SqliteDbContextFactory(SqliteConnection connection) => this.connection = connection;

        public PixelynxContext CreateAdmin()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseSqlite(connection)
                .Options;
            
            var context = new PixelynxContext(options, null);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return new PixelynxContext(options, null);
        }

        public PixelynxContext CreateRead()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseSqlite(connection)
                .Options;
            
            var context = new PixelynxContext(options, null);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return new PixelynxContext(options, null);
        }

        public PixelynxContext CreateReadWrite()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseSqlite(connection)
                .Options;
            
            var context = new PixelynxContext(options, null);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return new PixelynxContext(options, null);
        }

        public PixelynxContext CreateWrite()
        {
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseSqlite(connection)
                .Options;
            
            var context = new PixelynxContext(options, null);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return new PixelynxContext(options, null);
        }
    }
}