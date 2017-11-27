using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace jwt_sample
{
    public class LoginMiddleware
    {
        private readonly JwtSettings settings;
        private readonly JwtSecurityTokenHandler jwtTokenHandler;
        private SigningCredentials signingCredentials;
       
        public LoginMiddleware(RequestDelegate next, JwtSettings settings)
        {
            this.settings = settings;
            signingCredentials = new SigningCredentials(settings.Key, settings.Algorithm);
            jwtTokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task Invoke(HttpContext context)
        {
            var req = ParseRequest(context);

            if (!ValidateCredentials(req)) {
                context.Response.StatusCode = 401;  
                return;
            }
 
            context.User = new ClaimsPrincipal(new ClaimsIdentity( new [] { new Claim("sub", req.Username) }));
            
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(
                settings.Issuer, 
                settings.Audience, 
                context.User.Identities.First(), 
                signingCredentials: signingCredentials);

            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(jwtTokenHandler.WriteToken(jwtToken));
        }

        private bool ValidateCredentials(LoginRequest req) => req.Username == "admin" && req.Password == "one-jwt-token-please";

        private LoginRequest ParseRequest(HttpContext context) 
        {
            if (context.Request.ContentType == "application/json") return ParseJsonRequest(context);
            if (context.Request.ContentType == "application/x-www-form-urlencoded") return ParseFormRequest(context);

            return null;
        }

        private static LoginRequest ParseFormRequest(HttpContext context) => new LoginRequest {
            Username = context.Request.Form["username"],
            Password = context.Request.Form["password"]
        };

        private static LoginRequest ParseJsonRequest(HttpContext context) =>
             JsonConvert.DeserializeObject<LoginRequest>(
                 new StreamReader(context.Request.Body).ReadToEnd(), 
                 new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
    }
}
