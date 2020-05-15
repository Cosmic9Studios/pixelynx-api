using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixelynx.Logic.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntent(Guid userId, int total, Dictionary<string, string> metadata);
    }
}