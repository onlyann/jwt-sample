using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace jwt_sample
{
    public class DenyAnonymousMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IAuthorizationService authzService;
        private readonly AuthorizationPolicy denyAnonymousPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        public DenyAnonymousMiddleware(RequestDelegate next, IAuthorizationService authzService)
        {
            this.next = next;
            this.authzService = authzService;
        }

        public async Task Invoke(HttpContext context)
        {
            // verify that the user is authenticated
            var result = await authzService.AuthorizeAsync(context.User, resource: null, policy: denyAnonymousPolicy);

            if (!result.Succeeded) {
                context.Response.StatusCode = 401;
                return;
            }
            
            await next(context);
        }
    }
}
