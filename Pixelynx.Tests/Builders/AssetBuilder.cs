using System;
using System.Threading.Tasks;
using Pixelynx.Data;
using Pixelynx.Data.Entities;

namespace Pixelynx.Tests.Builders
{
    public class AssetBuilder
    {
        private AssetEntity asset;
        private readonly PixelynxContext context;
        public AssetBuilder(PixelynxContext context) => this.context = context;
        
        public AssetBuilder New(string name)
        {
            asset = new AssetEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = 0
            };

            return this;
        }

        public AssetBuilder WithId(Guid id)
        {
            asset.Id = id;
            return this;
        }

        public AssetBuilder WithPrice(int price)
        {
            asset.Price = price;
            return this;
        }

        public async Task<AssetEntity> BuildAndInsert()
        {
            await context.Assets.AddAsync(asset);
            await context.SaveChangesAsync();
            return asset;
        }
    }
}