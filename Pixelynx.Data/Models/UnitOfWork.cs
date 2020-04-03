using Microsoft.Extensions.Options;
using Pixelynx.Data.BlobStorage;
using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Repositories;
using Pixelynx.Data.Settings;

namespace Pixelynx.Data.Models
{
    public class UnitOfWork
    {
        private static IBlobStorage storage;
        private static StorageSettings storageSettings;

        public UnitOfWork(IDbContextFactory dbContextFactory, IBlobStorage blobStorage, IOptions<StorageSettings> settings)
        {
            storage = blobStorage;
            storageSettings = settings.Value;

            AssetRepository = new AssetRepository(dbContextFactory, storage, storageSettings.BucketName);
            PaymentRepository = new PaymentRepository(dbContextFactory);
        }

        public AssetRepository AssetRepository { get; }
        public PaymentRepository PaymentRepository { get; }
    }
}