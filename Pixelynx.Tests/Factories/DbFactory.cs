using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Pixelynx.Data;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;

namespace Pixelynx.Tests.Factories
{
    public static class DbFactory
    {
        public static UnitOfWork GetSqlLiteInMemoryDb(IBlobStorage blobStorage)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseSqlite(connection)
                .Options;

            var context = new PixelynxContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return new UnitOfWork(context, blobStorage, Options.Create(new StorageSettings()));
        }

        public static UnitOfWork GetInMemoryDb(IBlobStorage blobStorage)
        {
            return new UnitOfWork(new PixelynxContext(new DbContextOptionsBuilder<PixelynxContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options), blobStorage, Options.Create(new StorageSettings()));
        }
    }
}