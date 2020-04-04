using System.Threading.Tasks;
using Pixelynx.Logic.Settings;

namespace Pixelynx.Logic.Interfaces
{
    public interface IVaultService
    {
        Task<VaultAuthSettings> GetAuthSecrets();
    }
}