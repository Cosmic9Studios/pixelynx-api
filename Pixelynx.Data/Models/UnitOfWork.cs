using System;
using System.Threading.Tasks;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Repositories;

namespace Pixelynx.Data.Models
{
    public class UnitOfWork
    {
        private static PixelynxContext context;
        private static IBlobStorage storage;

        public UnitOfWork(PixelynxContext pixelynxContext, IBlobStorage blobStorage)
        {
            context = pixelynxContext;
            storage = blobStorage;
        }

        public Lazy<AssetRepository> AssetRepository = new Lazy<AssetRepository>(() => new AssetRepository(context, storage));

        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }
    }
}