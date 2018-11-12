using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace UpDiddyLib.Shared
{
    public class KeyVaultSecretManager : IKeyVaultSecretManager
    {
        public string GetKey(SecretBundle secret)
        {
            // Replace Azure default double dash "--" with configuration's key delimiter
            return secret.SecretIdentifier.Name
                .Replace("--", ConfigurationPath.KeyDelimiter);
        }

        // todo: could potentially add APP_VERSION prefix to secrets, this may offer some flexibility when deploying or testing
        public bool Load(SecretItem secret)
        {
            // always load the secret
            return true;
        }
    }
}
