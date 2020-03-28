using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Repositories;
using Pixelynx.Data.Settings;

namespace Pixelynx.Data.Models
{
    public class UnitOfWork
    {
        private static IBlobStorage storage;
        private static StorageSettings storageSettings;

        public UnitOfWork(DbContextFactory dbContextFactory, IBlobStorage blobStorage, IOptions<StorageSettings> settings)
        {
            storage = blobStorage;
            storageSettings = settings.Value;

            AssetRepository = new AssetRepository(dbContextFactory, storage, storageSettings.BucketName);
        }

        public AssetRepository AssetRepository { get; }
    }
}