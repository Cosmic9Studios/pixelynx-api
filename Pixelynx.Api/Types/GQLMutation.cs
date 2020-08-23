using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pixelynx.Api.Extensions;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
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
            string email, string password) => 
                await authService.Login(email, password, (await vaultService.GetAuthSecrets()).JWTSecret);
        

        [Authorize]
        public async Task<bool> Logout([Service] IAuthService authService) => authService.Logout();

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
        
        [Authorize]
        public IEnumerable<GQLAsset> Download(IReadOnlyList<Guid> assetIds, 
            [Service] IDbContextFactory dbContextFactory,
            [Service] IHttpContextAccessor contextAccessor)
        {
            using (var context = dbContextFactory.CreateRead())
            {
                var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
                var assets = context.PurchasedAssets.Where(x => x.UserId == userId)
                    .Select(x => x.Asset)
                    .Concat(context.Assets.Where(x => x.UploaderId == userId))
                    .Where(x => assetIds.Any(id => x.Id == id))
                    .ToGQLAsset()
                    .ToList();

                return assets;
            }
        }

        [Authorize]
        public async Task<GenericResult> AddToCart(
            [Service] IHttpContextAccessor contextAccessor, 
            [Service] ICartService cartService,
            IReadOnlyList<Guid> assetIds)
        {
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            await cartService.AddToCart(userId, assetIds);
            return new GenericResult
            {
                Succeeded = true
            };
        }

        [Authorize]
        public async Task<GenericResult> RemoveFromCart(
            [Service] ICartService cartService,
            [Service] IHttpContextAccessor contextAccessor,
            IReadOnlyList<Guid> assetIds)
        {
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            await cartService.RemoveFromCart(userId, assetIds);
            return new GenericResult
            {
                Succeeded = true
            };
        }

        [Authorize]
        public async Task CloseCart(
            [Service] ICartService cartService,
            [Service] IHttpContextAccessor contextAccessor)
        {
            var userId = Guid.Parse(contextAccessor.HttpContext.User.Identity.Name);
            await cartService.UpdateCartStatus(userId, CartStatus.Complete);
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
            [Service] ICartService cartService,
            [Service] IHttpContextAccessor contextAccessor, bool? useCredits)
        {
            if (!Guid.TryParse(contextAccessor.HttpContext.User.Identity.Name, out var userId))
            {
                return null;
            }

            var result = await cartService.Checkout(userId, useCredits.HasValue && useCredits.Value);
            var response = new PurchaseResponse
            {
                Succeeded = result.Succeeded,
                Error = result.Error,
            };

            if (string.IsNullOrEmpty(result.Error))
            {
                response.Data = result.Total == 0
                    ? "free"
                    : await paymentService.CreatePaymentIntent(userId, (int) result.Total,
                        new Dictionary<string, string>
                        {
                            {"type", "ASSETS"},
                            {"userId", userId.ToString()},
                            {"assets", JsonConvert.SerializeObject(result.AssetMetadata)},
                        });
            }

            return response;
        }
    }
}