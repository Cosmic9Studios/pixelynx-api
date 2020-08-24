using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Pixelynx.Data;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Models;

namespace Pixelynx.Tests.Factories
{
    public static class DbFactory
    {
        public static UnitOfWork GetSqlLiteInMemoryDb(IBlobStorage blobStorage)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var dbContextFactory = new SqliteDbContextFactory(connection);

            return new UnitOfWork(dbContextFactory);
        }

        public static UnitOfWork GetInMemoryDb(IBlobStorage blobStorage)
        {
            var dbContextFactory = new InMemoryDbContextFactory();
            return new UnitOfWork(dbContextFactory);
        }

        public static IDbContextFactory GetInMemoryDbContext()
        {
            return new InMemoryDbContextFactory();
        }

        public static IDbContextFactory GetSqlLiteDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            return new SqliteDbContextFactory(connection);
        }
    }
}