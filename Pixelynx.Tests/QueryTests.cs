using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pixelynx.Api.Types;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Tests.Factories;
using Pixelynx.Tests.Mocks;
using Xunit;

namespace Pixelynx.Tests
{
    public class QueryTests : IAsyncLifetime
    {
        private IDbContextFactory dbContextFactory;
        private const string storageId1 = "fec9f5ef-0d95-4bcc-902d-e9b45766925a";
        private const string storageId2 = "fec9f5ef-0d95-4bcc-902d-e9b45766925b";
        private MockBlobStorage blobStorage;

        public QueryTests()
        {
            blobStorage = new MockBlobStorage();
            dbContextFactory = new InMemoryDbContextFactory();
        }

        public async Task InitializeAsync()
        {
            await blobStorage.UploadFileToBucket("", storageId1, "asset.glb", null);
            await blobStorage.UploadFileToBucket("", storageId1, "thumbnail.png", null);
            await blobStorage.UploadFileToBucket("", storageId1, "watermark.glb", null);

            await blobStorage.UploadFileToBucket("", storageId2, "asset.glb", null);
            await blobStorage.UploadFileToBucket("", storageId2, "thumbnail.png", null);
            await blobStorage.UploadFileToBucket("", storageId2, "watermark.glb", null);

            var writeContext = dbContextFactory.CreateWrite();
            writeContext.Assets.Add(new AssetEntity
            {
                Name = "robot",
                AssetType = (int) Core.AssetType.Model,
                StorageId = Guid.Parse(storageId1)
            });

            writeContext.Assets.Add(new AssetEntity
            {
                Name = "foo",
                AssetType = (int) Core.AssetType.Model,
                StorageId = Guid.Parse(storageId2)
            });

            await writeContext.SaveChangesAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Assets_ShouldReturnAllAssetsInStorage()
        {
            var query = new GQLQuery();
            var result = await query.GetAssets(dbContextFactory, 0, 0).ToListAsync();

            result.Should().HaveCount(2);
        }
    }
}