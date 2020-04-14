using System.Threading.Tasks;
using Pixelynx.Data.Settings;
using Pixelynx.Data.Interfaces;
using VaultSharp;
using Pixelynx.Core.Helpers;
using System.Collections.Generic;

namespace Pixelynx.Logic.Services
{
    public class VaultService : IVaultService
    {
        private IVaultClient vaultClient;

        public VaultService(IVaultClient vaultClient)
        {
            this.vaultClient = vaultClient;
        }

        public async Task<VaultAuthSettings> GetAuthSecrets()
        {
            var secrets = await vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync("/Auth");
            return secrets.Data.ToObject<VaultAuthSettings>();
        }

        public async Task<KeyValuePair<string, string>> GetDbCredentials(VaultRole role)
        {
            var dbCreds = await vaultClient.V1.Secrets.Database.GetCredentialsAsync(role.ToString().ToLower());
            return new KeyValuePair<string, string>(dbCreds.Data.Username, dbCreds.Data.Password);
        }
    }
}