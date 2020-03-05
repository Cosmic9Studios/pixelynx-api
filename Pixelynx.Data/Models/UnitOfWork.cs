using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Repositories;
using Pixelynx.Data.Settings;

namespace Pixelynx.Data.Models
{
    public class UnitOfWork
    {
        private static PixelynxContext context;
        private static IBlobStorage storage;
        private static StorageSettings storageSettings;

        public UnitOfWork(PixelynxContext pixelynxContext, IBlobStorage blobStorage, IOptions<StorageSettings> settings)
        {
            context = pixelynxContext;
            storage = blobStorage;
            storageSettings = settings.Value;

            AssetRepository = new AssetRepository(context, storage, storageSettings.BucketName);
        }

        public AssetRepository AssetRepository { get; }

        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }
    }
}