using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace jwt_sample 
{
    /// <summary>
    /// Restricts the default CryptoProviderFactory to a list of supported algorithms.
    /// </summary>
    public class RestrictedCryptoProviderFactory : CryptoProviderFactory 
    {
        public HashSet<string> SupportedAlgorithms { get; private set; } = new HashSet<string>();

        public RestrictedCryptoProviderFactory() 
        {
        }

        public RestrictedCryptoProviderFactory(IEnumerable<string> supportedAlgorithms) 
        {
            SupportedAlgorithms.UnionWith(supportedAlgorithms);
        }

        public override bool IsSupportedAlgorithm(string algorithm, SecurityKey key) 
        {
            return SupportedAlgorithms.Contains(algorithm) && base.IsSupportedAlgorithm(algorithm, key);
        }
    }
}