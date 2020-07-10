using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Pixelynx.Api.Extensions;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Models;
using Stripe;

namespace Pixelynx.Api.Types
{
    public class GQLMutation
    {
        public async Task<string> Login(
            [Service] IAuthService authService, 
            [Service] IVaultService vaultService,
            string email, string password)
        {
            var authSettings = await vaultService.GetAuthSecrets();
            return await authService.Login(email, password, (await vaultService.GetAuthSecrets()).JWTSecret);
        }

        [Authorize]
        public async Task<bool> Logout([Service] IAuthService authService) => 
            await authService.Logout();

        public async Task<GenericResult<string>> Register(
            [Service] IAuthService authService, 
            [Service] IHttpContextAccessor contextAccessor,
            RegistrationRequest user) =>
                await authService.Register(contextAccessor.HttpContext.Request, new UserEntity 
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email
                }, user.Password);
        
        public async Task<GenericResult<string>> ConfirmEmail(
            [Service] IAuthService authService,
            [Service] IDbContextFactory dbContextFactory,
            string userId, string code, string type) =>
                await authService.ConfirmEmail(dbContextFactory, userId, code, type);
        
        public async Task<bool> ResendEmail(
            [Service] IAuthService authService,
            [Service] IHttpContextAccessor contextAccessor,
            string userId = "", string email = "") =>
                await authService.ResendEmail(contextAccessor.HttpContext.Request, userId, email);
        
        public async Task<bool> ForgotPassword(
            [Service] IAuthService authService, 
            [Service] IHttpContextAccessor contextAccessor, 
            string email) =>
                await authService.ForgotPassword(contextAccessor.HttpContext.Request, email);
        
        public async Task<GenericResult<string>> ResetPassword(
            [Service] IAuthService authService,
            string userId, string code, string newPassword) => 
                await authService.ResetPassword(userId, code, newPassword);
        
        [Authorize]
        public async Task<GenericResult<string>> UpdatePassword(
            [Service] IAuthService authService,
            [Service] IHttpContextAccessor contextAccessor,
            string oldPassword, string newPassword) => 
                await authService.UpdatePassword(contextAccessor.HttpContext.User.Identity.Name, 
                    oldPassword, newPassword);
        
        [Authorize]
        public async Task<bool> SetDefaultPaymentMethod(
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor,
            string paymentMethodId)
        {
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            using (var context = dbContextFactory.CreateReadWrite())
            {
                var paymentDetails = await context.PaymentDetails.FirstAsync(x => x.UserId == userId);
                paymentDetails.DefaultPaymentMethodId = paymentMethodId;
                context.Update(paymentDetails);
                await context.SaveChangesAsync();
            }

            return true;
        }

        [Authorize]
        public async Task<string> AddCard(
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor)
        {
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            string customer;
            using (var context = dbContextFactory.CreateRead())
            {
                customer = (await context.PaymentDetails.FirstAsync(x => x.UserId == userId)).CustomerId;
            }

            var options = new SetupIntentCreateOptions{
                Customer = customer
            };
    
            var service = new SetupIntentService();
            var intent = await service.CreateAsync(options);
            return intent.ClientSecret;
        }

        [Authorize]
        public bool RemoveCard(string cardId)
        {
            var service = new PaymentMethodService();
            service.Detach(cardId);
            return true;
        }

        public async Task<IEnumerable<GQLAsset>> Download(IReadOnlyList<Guid> assetIds, 
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor,
            [Service] UserManager<UserEntity> userManager)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var email = contextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                UserEntity user = null;
                if (email != null)
                {
                    user = await userManager.FindByEmailAsync(email);
                }

                var purchasedAssets = await context.PurchasedAssets.ToListAsync();
                var assets = context.Assets.Where(x => assetIds.Any(id => x.Id == id)).ToGQLAsset();

                bool notPurchased = assets.Any(x =>
                {
                    return x.Cost != 0 && purchasedAssets.All(asset => asset.AssetId != x.Id);
                });
                
                return notPurchased ? new List<GQLAsset>() : assets;
            }
        }

        [Authorize]
        public async Task<bool> AddToCart(
            [Service] IDbContextFactory dbContextFactory, 
            [Service] IHttpContextAccessor contextAccessor, 
            List<Guid> assetIds)
        {
            var dbContext = dbContextFactory.CreateReadWrite();
            foreach (var assetId in assetIds)
            {
                var asset = await dbContext.Assets.FirstOrDefaultAsync(x => x.Id == assetId);
                if (asset == null)
                {
                    return false;
                }

                var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
                var cart = await dbContext.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
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

                var isItemInCart = await dbContext.CartItems.AnyAsync(x => x.AssetId == assetId);
                if (!isItemInCart)
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

            return true;
        }

        [Authorize]
        public async Task<bool> RemoveFromCart(
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor,
            List<Guid> assetIds)
        {
            var dbContext = dbContextFactory.CreateReadWrite();
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            var cart = await dbContext.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
            var assetsToRemove = dbContext.CartItems.Where(x =>
                x.CartId == cart.Id && assetIds.Any(aId => aId == x.AssetId)).ToList();
            
            dbContext.CartItems.RemoveRange(assetsToRemove);
            await dbContext.SaveChangesAsync();

            return true;
        }

        [Authorize]
        public async Task<string> AddCredits(
            [Service] IPaymentService paymentService,
            [Service] IHttpContextAccessor contextAccessor,
            int amount)
        {
            if (!Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId) || amount < 20)
            {
                return null;
            }

            return await paymentService.CreatePaymentIntent(userId, amount, new Dictionary<string, string>
            {
                { "type", "CREDITS" },
                { "userId", userId.ToString() },
                { "amount", amount.ToString() },
            });
        }

        [Authorize]
        public async Task<PurchaseResponse> PurchaseAssets(
            [Service] IPaymentService paymentService,
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor, bool? useCredits)
        {
            if (!Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId))
            {
                return null;
            }
            
            var context = dbContextFactory.CreateReadWrite();
            var cart = await context.Carts.LastOrDefaultAsync(x => x.UserId == userId && x.Status == CartStatus.New);
            if (cart == null)
            {
                return null;
            }

            var assetsToPurchase = context.CartItems
                .Include(x => x.Asset)
                .Where(x => x .CartId == cart.Id)
                .Select(x => x.Asset.Id);
            
            var user = context.Users.First(x => x.Id == userId);
            var total = assetsToPurchase.Sum(id => context.Assets.First(x => x.Id == id).Price);

            if (useCredits != null && useCredits.Value == true)
            {
                var credits = user.Credits;
                if (credits < total) 
                {
                    return new PurchaseResponse
                    {
                        Succeeded = false,
                        Error = "Insufficient Credits"
                    };
                }
                else 
                {
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

                    return new PurchaseResponse
                    {
                        Succeeded = true
                    };
                }
            }
            else 
            {
                return new PurchaseResponse 
                {
                    Succeeded = true,
                    Data = await paymentService.CreatePaymentIntent(userId, (int)total + 2, new Dictionary<string, string>
                    {
                        { "type", "ASSETS" },
                        { "userId", userId.ToString() },
                        { "assets", string.Join(',', assetsToPurchase) },
                    })
                };
            }
        }
    }
}