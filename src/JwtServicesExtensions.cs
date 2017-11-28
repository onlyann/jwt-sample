using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace jwt_sample {
    public static class JwtServicesExtensions 
    {
        /// <summary>
        /// Registers services for authentication from a JWT Bearer token
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        public static void AddJwtAuthentication(this IServiceCollection services, JwtSettings settings) 
        {
            settings.InitializeKey();
            services.AddSingleton(settings);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt => {
                    opt.TokenValidationParameters.IssuerSigningKey = settings.Key;
                    // by default the JWT validator will use the algorithm from the header to 
                    // verify the signature. Rather than accept all the algorithms from the standard, 
                    // we only accept our configured list of supported algorithms.
                    opt.TokenValidationParameters.CryptoProviderFactory = new RestrictedCryptoProviderFactory(settings.SupportedAlgorithms);
                    
                    // only tokens from the configured issuer will be accepted
                    opt.TokenValidationParameters.ValidIssuer = settings.Issuer;

                    // only tokens from the configured audience will be accepted
                    opt.TokenValidationParameters.ValidAudience = settings.Audience;

                    // use sub claim as the name for the created claims identity.
                    opt.TokenValidationParameters.NameClaimType = "sub";

                    // timespan tolerance when validating token expiration date
                    opt.TokenValidationParameters.ClockSkew = settings.ClockSkew;

                    var tokenValidator = new JwtSecurityTokenHandler();
                    // this prevents the validator from automatically mapping jwt claims into Microsoft own claim types
                    // by default "sub" would be mapped into http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
                    tokenValidator.InboundClaimTypeMap.Clear();

                    // replace the default validator
                    opt.SecurityTokenValidators.Clear();
                    opt.SecurityTokenValidators.Add(tokenValidator);
                });
        }
    }
}