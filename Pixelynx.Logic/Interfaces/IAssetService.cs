using System;

namespace Pixelynx.Logic.Interfaces
{
    public interface IAssetService
    {
        bool IsOwned(Guid userId, Guid assetId);
    }
}