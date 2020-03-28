using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Pixelynx.Data;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;

namespace Pixelynx.Tests.Factories
{
    public static class DbFactory
    {
        public static UnitOfWork GetSqlLiteInMemoryDb(IBlobStorage blobStorage)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var dbContextFactory = new SqliteDbContextFactory(connection);

            return new UnitOfWork(dbContextFactory, blobStorage, Options.Create(new StorageSettings()));
        }

        public static UnitOfWork GetInMemoryDb(IBlobStorage blobStorage)
        {
            var dbContextFactory = new InMemoryDbContextFactory();
            return new UnitOfWork(dbContextFactory, blobStorage, Options.Create(new StorageSettings()));
        }
    }
}