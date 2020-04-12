using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pixelynx.Data;
using Pixelynx.Data.Interfaces;

namespace Pixelynx.Tests.Factories
{
    public class InMemoryDbContextFactory : IDbContextFactory
    {
        private DbContextOptions<PixelynxContext> options;
        public InMemoryDbContextFactory() 
        {
            options = new DbContextOptionsBuilder<PixelynxContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }
        public PixelynxContext Create()
        {       
            var context = new PixelynxContext(options, null);

            return new PixelynxContext(options, null);
        }
    }
}