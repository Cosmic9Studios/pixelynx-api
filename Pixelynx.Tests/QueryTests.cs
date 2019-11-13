using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Pixelynx.Api.Settings;
using Pixelynx.Api.Types;
using Pixelynx.Data.BlobStorage;
using Xunit;

namespace Pixelynx.Tests
{
    public class QueryTests
    {
        private Mock<IBlobStorage> blobStorageMock;

        public QueryTests()
        {
            blobStorageMock = new Mock<IBlobStorage>();
            blobStorageMock.Setup(x => x.ListObjects(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<BlobObject>
                {
                    new BlobObject { Key = "robot/image.png", Uri="http://assetstoretest.com/house/image.png"},
                    new BlobObject { Key = "robot/asset.glb", Uri="http://assetstoretest.com/robot/asset.glb"},
                    new BlobObject { Key = "house/image.png", Uri="http://assetstoretest.com/house/image.png"},
                    new BlobObject { Key = "house/asset.glb", Uri="http://assetstoretest.com/house/asset.glb"},
                } as IEnumerable<BlobObject>));
        }

        [Fact]
        public async Task Assets_ShouldReturnAllAssetsInStorage()
        {
            var query = new Query();
            var result = await query.GetAssets(blobStorageMock.Object, Options.Create(new AssetstoreSettings()), "");

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Assets_ShouldOnlyReturnAssetsThatContainFilter()
        {
            var query = new Query();
            var result = await query.GetAssets(blobStorageMock.Object, Options.Create(new AssetstoreSettings()), "robot");

            result.Should().HaveCount(1);
        }
    }
}
