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

            // registers JWT services for authentication and or signing
            services.AddJwtAuthentication(settings);

            // register ASP.NET Core authorization services
            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Func<HttpContext, bool> isLoginPost = ctx => ctx.Request.Path == "/login" && ctx.Request.Method == "POST";

            // middleware that will process POST requests at /login and will return a JWT token
            app.MapWhen(isLoginPost, appOnLoginPost => { appOnLoginPost.UseMiddleware<LoginMiddleware>(); });

            // if a Bearer token is present and signature is valid, 
            //a user will be set on HttpContext.User (ie authenticated request)
            app.UseAuthentication();

            // pass-through if request is authenticated, otherwise returns a 401 response.
            app.UseMiddleware<DenyAnonymousMiddleware>();

            // only authenticated requests will run this middleware
            app.Run(context => context.Response.WriteAsync($"Hello {context.User.Identity.Name}!"));
        }
    }
}
