using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace jwt_sample
{
    public class Startup
    {           
        public IConfiguration Configuration { get;}

        public Startup(IHostingEnvironment env) {   
            Configuration = new ConfigurationBuilder()   
                .SetBasePath(env.ContentRootPath)  
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .Build();   
        }   

        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.Get<JwtSettings>();
            settings.Initialize();

            services.AddSingleton(settings);

            var tokenValidator = new JwtSecurityTokenHandler();
            tokenValidator.InboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt => {
                    opt.TokenValidationParameters.IssuerSigningKey = settings.Key;
                    opt.TokenValidationParameters.ValidIssuer = settings.Issuer;
                    opt.TokenValidationParameters.ValidAudience = settings.Audience;
                    opt.TokenValidationParameters.NameClaimType = "sub";
                    opt.SecurityTokenValidators.Clear();
                    opt.SecurityTokenValidators.Add(tokenValidator);
                });
            
            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Func<HttpContext, bool> isLoginPost = ctx => ctx.Request.Path == "/login" && ctx.Request.Method == "POST";

            app.MapWhen(isLoginPost, appOnLoginPost => { appOnLoginPost.UseMiddleware<LoginMiddleware>(); });

            app.UseAuthentication();

            app.UseMiddleware<DenyAnonymousMiddleware>();

            app.Run(context => context.Response.WriteAsync($"Hello {context.User.Identity.Name}!"));
        }
    }
}
