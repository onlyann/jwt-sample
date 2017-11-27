using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace jwt_sample {
    public class JwtSettings {

        static RandomNumberGenerator rng = RandomNumberGenerator.Create();
        static HashSet<string> SupportedAlgorithms = new HashSet<string>(
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
        public string SecretBase64 {get; set;}
        public string RsaKeysXml {get; set;}

        public SecurityKey Key {get; set;}

        public bool IsAsymmetricalAlgo { get => Algorithm?.StartsWith("RS") ?? false; }
        public void Initialize() 
        {
            Algorithm = Algorithm ?? SecurityAlgorithms.HmacSha256;
            if (!SupportedAlgorithms.Contains(Algorithm))
                throw new ArgumentException(nameof(Algorithm), $"{Algorithm} is not supported");

            Key = IsAsymmetricalAlgo ? GetOrCreateRsaKey() : GetOrCreateSymmetricKey();
        }

        public int AlgoKeySize 
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

        public SecurityKey GetOrCreateSymmetricKey()
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

        public SecurityKey GetOrCreateRsaKey() 
        {
            var rsa = RSA.Create();

            if (RsaKeysXml != null)
                rsa.FromXmlString(RsaKeysXml);
            else
                rsa.KeySize = AlgoKeySize;

            return new RsaSecurityKey(rsa);
        }
    }
}