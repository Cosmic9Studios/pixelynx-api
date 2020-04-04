using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault;
using Microsoft.Extensions.Configuration;
using Pixelynx.Logic.Settings;
using Pixelynx.Logic.Interfaces;
using VaultSharp;
using System;
using Pixelynx.Core.Helpers;

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
    }
}