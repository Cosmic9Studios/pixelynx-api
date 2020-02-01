using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Types;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;
using Pixelynx.Tests.Factories;
using Pixelynx.Tests.Mocks;
using Xunit;

namespace Pixelynx.Tests
{
    public class QueryTests : IAsyncLifetime
    {
        private UnitOfWork unitOfWork;
        private const string storageId1 = "fec9f5ef-0d95-4bcc-902d-e9b45766925a";
        private const string storageId2 = "fec9f5ef-0d95-4bcc-902d-e9b45766925b";
        private MockBlobStorage blobStorage;

        public QueryTests()
        {
            blobStorage = new MockBlobStorage();
            unitOfWork = DbFactory.GetInMemoryDb(blobStorage);
        }

        public async Task InitializeAsync()
        {
            await blobStorage.UploadFileToBucket("", storageId1, "asset.glb", null);
            await blobStorage.UploadFileToBucket("", storageId1, "thumbnail.png", null);
            await blobStorage.UploadFileToBucket("", storageId1, "watermark.glb", null);

            await blobStorage.UploadFileToBucket("", storageId2, "asset.glb", null);
            await blobStorage.UploadFileToBucket("", storageId2, "thumbnail.png", null);
            await blobStorage.UploadFileToBucket("", storageId2, "watermark.glb", null);

            await unitOfWork.AssetRepository.Value.CreateAsset(new Core.Asset("robot", Core.AssetType.Model, storageId1));
            await unitOfWork.AssetRepository.Value.CreateAsset(new Core.Asset("foo", Core.AssetType.Model, storageId2));
            await unitOfWork.SaveChanges();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Assets_ShouldReturnAllAssetsInStorage()
        {
            var query = new Query(unitOfWork);
            var result = await query.GetAssets(blobStorage, Options.Create(new StorageSettings()), "", "", "");

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Assets_ShouldOnlyReturnAssetsThatContainFilter()
        {
            var query = new Query(unitOfWork);
            var result = await query.GetAssets(blobStorage, Options.Create(new StorageSettings()), "robot", "", "");

            result.Should().HaveCount(1);
        }
    }
}
