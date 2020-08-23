using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Services;
using Xunit;
using FluentAssertions;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Tests.Builders;
using Pixelynx.Tests.Factories;

namespace Pixelynx.Tests.Pixelynx.Logic.Services
{
    public class CartService_Tests : IAsyncLifetime
    {
        private readonly ICartService cartService;
        private readonly IDbContextFactory dbContextFactory;
        private readonly AssetBuilder assetBuilder;
        private readonly UserBuilder userBuilder;
        private readonly List<AssetEntity> assets;
        private UserEntity user;

        public CartService_Tests()
        {
            dbContextFactory = DbFactory.GetInMemoryDbContext();
            cartService = new CartService(dbContextFactory, new AssetService(dbContextFactory));
            
            var writeContext = dbContextFactory.CreateReadWrite();
            assetBuilder = new AssetBuilder(writeContext);
            userBuilder = new UserBuilder(writeContext);
            
            assets = new List<AssetEntity>();
        }
        
        public async Task InitializeAsync()
        {
            assets.Add(await assetBuilder.New("Robot").WithPrice(1).BuildAndInsert());
            assets.Add(await assetBuilder.New("Sword").WithPrice(20).BuildAndInsert());
            user = await userBuilder.New("PixelBot").WithCredits(5).BuildAndInsert();
        }
        
        public async Task DisposeAsync()
        {
            var dbContext = dbContextFactory.CreateWrite();
            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task ShouldAddSmallOrderFee_NoCredits()
        {
            await cartService.AddToCart(user.Id, new List<Guid> { assets[0].Id });
            var result = await cartService.Checkout(user.Id, false);
            result.Total.Should().Be(3);
        }
        
        [Fact]
        public async Task ShouldNotAddSmallOrderFee_NoCredits()
        {
            await cartService.AddToCart(user.Id, new List<Guid> { assets[1].Id });
            var result = await cartService.Checkout(user.Id, false);
            result.Total.Should().Be(20);
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotAddSmallOrderFree_Credits()
        {
            await cartService.AddToCart(user.Id, new List<Guid> { assets[0].Id });
            var result = await cartService.Checkout(user.Id, true);
            result.Total.Should().Be(1);
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnError_InsufficientCredits()
        {
            await cartService.AddToCart(user.Id, new List<Guid> { assets[1].Id });
            var result = await cartService.Checkout(user.Id, true);
            result.Succeeded.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ShouldCreateNewCart_AfterClose()
        {
            await cartService.AddToCart(user.Id, new List<Guid> { assets[1].Id });
            await cartService.CloseCart(user.Id, CartStatus.Complete);
            await cartService.AddToCart(user.Id, new List<Guid> { assets[1].Id });

            await using var dbContext = dbContextFactory.CreateRead();
            var carts = dbContext.Carts.Where(x => x.UserId == user.Id).ToList();
            carts.Count.Should().Be(2);
            carts.Count(x => x.Status == CartStatus.New).Should().Be(1);
            carts.Count(x => x.Status == CartStatus.Complete).Should().Be(1);
        }

        [Fact]
        public async Task ShouldCloseCartAfterCheckout_Credits()
        {
            await cartService.AddToCart(user.Id, new List<Guid> { assets[0].Id });
            await cartService.Checkout(user.Id, true);
            (await cartService.GetCartItems(user.Id)).Count().Should().Be(0);
        }

        [Fact]
        public async Task ShouldAddItemsToCart()
        {
            await cartService.AddToCart(user.Id, assets.Select(x => x.Id));
            await using var dbContext = dbContextFactory.CreateRead();
            (await cartService.GetCartItems(user.Id)).Count().Should().Be(2);
        }

        [Fact]
        public async Task ShouldNotAddToCart_Owned()
        {
            var dbContext = dbContextFactory.CreateReadWrite();
            await dbContext.PurchasedAssets.AddAsync(new PurchasedAssetEntity
            {
                Id = Guid.NewGuid(),
                AssetId = assets[0].Id,
                UserId = user.Id
            });

            await dbContext.SaveChangesAsync();
            await cartService.AddToCart(user.Id, assets.Select(x => x.Id));

            (await cartService.GetCartItems(user.Id)).Count().Should().Be(1);
        }

        [Fact]
        public async Task ShouldRemoveFromCart()
        {
            await cartService.AddToCart(user.Id, assets.Select(x => x.Id));
            await cartService.RemoveFromCart(user.Id, new List<Guid> { assets[0].Id });
            (await cartService.GetCartItems(user.Id)).Count().Should().Be(1);
            (await cartService.GetCartItems(user.Id)).ElementAt(0).AssetId.Should().Be(assets[1].Id);
        }
    }
}