using System.Collections.Generic;
using System.Threading.Tasks;
using Pixelynx.Data.Settings;

namespace Pixelynx.Data.Interfaces
{
    public enum VaultRole
    {
        ADMIN,
        READ,
        WRITE,
        READ_WRITE
    }
    public interface IVaultService
    {
        Task<VaultAuthSettings> GetAuthSecrets();
        Task<KeyValuePair<string, string>> GetDbCredentials(VaultRole role);
    }
}