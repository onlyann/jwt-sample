using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace jwt_sample {
    /// <summary>
    /// Holds JWT settings used for JWT validation and or signing.
    /// </summary>
    public class JwtSettings 
    {
        static RandomNumberGenerator rng = RandomNumberGenerator.Create();
        public HashSet<string> SupportedAlgorithms { get; protected set; } = new HashSet<string>(
            new[] {
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.HmacSha384,
                SecurityAlgorithms.HmacSha512,
                SecurityAlgorithms.RsaSha256,
                SecurityAlgorithms.RsaSha384,
                SecurityAlgorithms.RsaSha512});

        public string Audience {get; set;}
        public string Issuer {get; set;}
        public string Algorithm {get; set;}

        public int TokenLifetimeInMinutes { get; set; } = 30;
 
        public TimeSpan ClockSkew {get; set;} = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Base64 encoded key used by HMAC-SHA symmetrical algorithms.
        /// </summary>
        public string SecretBase64 {get; set;}

        /// <summary>
        /// XML representation of RSA private and/or public keys used by RSA algorithms
        /// </summary>
        public string RsaKeysXml {get; set;}

        /// <summary>
        /// Key used fr JWT signing and/or verification
        /// </summary>
        /// <returns></returns>
        public SecurityKey Key {get; set;}

        public bool IsAsymmetricalAlgo { get => Algorithm?.StartsWith("RS") ?? false; }

        /// <summary>
        /// Initializes the security key.
        /// If no algorithm is set, defaults to HMAC-SHA256.
        /// If no key is provided, will generate one on the fly based on the type of algorithm
        /// </summary>
        public void InitializeKey() 
        {
            Algorithm = Algorithm ?? SecurityAlgorithms.HmacSha256;
            if (!SupportedAlgorithms.Contains(Algorithm))
                throw new ArgumentException(nameof(Algorithm), $"{Algorithm} is not supported");

            Key = IsAsymmetricalAlgo ? GetOrCreateRsaKey() : GetOrCreateSymmetricKey();
        }

        private int AlgoKeySize 
        { 
            get 
            {
                if (Algorithm == null) return 0;
                if (Algorithm.StartsWith("RS")) return 2048;
                if (Algorithm.StartsWith("HS"))
                {
                    int.TryParse(Algorithm?.Substring(2),out int size);
                    return size;
                }
                
                return 0;
            }
        }

        private SecurityKey GetOrCreateSymmetricKey()
        {
            byte[] secret = null;

            if (SecretBase64 != null)
                secret = Convert.FromBase64String(SecretBase64);
            else
            {
                secret = new byte[AlgoKeySize];
                rng.GetNonZeroBytes(secret);
            }

            return new SymmetricSecurityKey(secret);
        }

        private SecurityKey GetOrCreateRsaKey() 
        {
            var rsa = RSA.Create();

            if (RsaKeysXml != null)
                rsa.FromXml(RsaKeysXml);
            else
                rsa.KeySize = AlgoKeySize;
                
            return new RsaSecurityKey(rsa);
        }
    }
}