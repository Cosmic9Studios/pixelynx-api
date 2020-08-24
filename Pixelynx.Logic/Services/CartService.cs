using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Models;

namespace Pixelynx.Logic.Services
{
    public class CartService : ICartService
    {
        private readonly IDbContextFactory dbContextFactory;
        private readonly IAssetService assetService;

        public CartService(IDbContextFactory dbContextFactory, IAssetService assetService)
        {
            this.dbContextFactory = dbContextFactory;
            this.assetService = assetService;
        }
        
        public async Task AddToCart(Guid userId, IEnumerable<Guid> assetIds)
        {
            var dbContext = dbContextFactory.CreateReadWrite();
            foreach (var assetId in assetIds)
            {
                var asset = await dbContext.Assets.FirstOrDefaultAsync(x => x.Id == assetId);
                if (asset == null)
                {
                    throw new InvalidOperationException($"Unable to find asset with id {assetId}");
                }
                
                var cart = await GetLatestCart(userId);
                if (cart == null)
                {
                    cart = new CartEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Status = CartStatus.New,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                    };
                    await dbContext.Carts.AddAsync(cart);
                    await dbContext.SaveChangesAsync();
                }

                var isItemInCart = await dbContext.CartItems.AnyAsync(x => x.CartId == cart.Id && x.AssetId == assetId);
                var owned = assetService.IsOwned(userId, assetId);
                if (!isItemInCart && !owned)
                {
                    await dbContext.CartItems.AddAsync(new CartItemEntity
                    {
                        AssetId = assetId,
                        CartId = cart.Id,
                        CreatedDate = DateTime.UtcNow,
                    });
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveFromCart(Guid userId, IEnumerable<Guid> assetIds)
        {
            var dbContext = dbContextFactory.CreateReadWrite();
            var cart = await GetLatestCart(userId);
            var assetsToRemove = cart.CartItems.Where(x => assetIds.Any(aId => aId == x.AssetId)).ToList();
            
            dbContext.CartItems.RemoveRange(assetsToRemove);
            await dbContext.SaveChangesAsync();
        }

        public async Task<CheckoutData> Checkout(Guid userId, bool useCredits = false)
        {
            var context = dbContextFactory.CreateReadWrite();
            var cart = await context.Carts.Where(x => x.UserId == userId && x.Status == CartStatus.New)
                .OrderByDescending(x => x.UpdatedDate).FirstOrDefaultAsync();
            if (cart == null)
            {
                return null;
            }

            var assetsToPurchase = context.CartItems
                .Include(x => x.Asset)
                .Where(x => x.CartId == cart.Id)
                .Select(x => x.Asset.Id);
            
            var user = context.Users.First(x => x.Id == userId);
            var total = assetsToPurchase.Sum(id => context.Assets.First(x => x.Id == id).Price);

            if (total == 0 || useCredits)
            {
                var credits = user.Credits;
                if (credits < total) 
                {
                    return new CheckoutData
                    {
                        Succeeded = false,
                        Error = "Insufficient Credits"
                    };
                }
 
                user.Credits -= (int)total;
                context.Users.Update(user);
                assetsToPurchase.ForEach(async id => {
                    await context.PurchasedAssets.AddAsync(new PurchasedAssetEntity
                    {
                        AssetId = id,
                        UserId = userId,
                        TransactionId = $"cred_${total}_${Guid.NewGuid().ToString()}",
                        Date = DateTime.UtcNow
                    });
                });
                
                await context.SaveChangesAsync();
                await UpdateCartStatus(userId, CartStatus.Complete);

                return new CheckoutData
                {
                    Succeeded = true,
                    Total = total
                };
            }
            
            var assetMetadata = context.Assets.Where(x => assetsToPurchase.Any(y => y == x.Id)).Select(a => new AssetMetadata
            {
                Id = a.Id,
                OwnerId = a.UploaderId,
                Cost = (int) a.Price,
            }).ToList();

            var tax = total >= 20 ? 0 : 2;

            return new CheckoutData
            {
                Succeeded = true,
                Total = total + tax,
                AssetMetadata = assetMetadata
            };
        }

        public async Task UpdateCartStatus(Guid userId, CartStatus status)
        {
            var context = dbContextFactory.CreateReadWrite();
            var cart = await GetLatestCart(userId);
            if (cart == null)
            {
                return;
            }

            cart.Status = status;
            cart.UpdatedDate = DateTime.UtcNow;
            context.Update(cart);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CartItemEntity>> GetCartItems(Guid userId) =>
            (await GetLatestCart(userId))?.CartItems ?? new List<CartItemEntity>();

        private async Task<CartEntity> GetLatestCart(Guid userId)
        {
            var context = dbContextFactory.CreateRead();
            return await context.Carts.Where(x => x.UserId == userId && x.Status == CartStatus.New)
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Asset)
                .OrderByDescending(x => x.UpdatedDate)
                .FirstOrDefaultAsync();
        }
    }
}