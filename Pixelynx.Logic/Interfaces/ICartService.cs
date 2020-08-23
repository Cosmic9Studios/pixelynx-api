using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pixelynx.Data.Entities;
using Pixelynx.Logic.Models;

namespace Pixelynx.Logic.Interfaces
{
    public interface ICartService
    {
        Task AddToCart(Guid userId, IEnumerable<Guid> assetIds);
        Task RemoveFromCart(Guid userId, IEnumerable<Guid> assetIds);
        Task<CheckoutData> Checkout(Guid userId, bool useCredits);
        Task UpdateCartStatus(Guid userId, CartStatus status);
        Task<IEnumerable<CartItemEntity>> GetCartItems(Guid userId);
    }
}