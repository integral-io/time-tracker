using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace TimeTracker.Api
{
    public static class AppRoles
    {
        public const string OrganizationMember = "organizationmember";
        public const string Admin = "admin";
    }
    public static class StartupExtensions
    {
        public static void ConfigureGoogleAuth(this IServiceCollection services, string clientId, string clientSecret)
        {
            services.AddIdentity<IdentityUser, IdentityRole>();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddGoogle(options =>
                {
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.SaveTokens = true;
                    options.Events.OnCreatingTicket = ctx =>
                    {
                        string email = ctx.Principal.FindFirstValue(
                            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                        if (email.EndsWith("@integral.io"))
                        {
                            ctx.Principal.AddIdentity(new ClaimsIdentity(
                                new List<Claim>
                                {
                                    new Claim(ClaimTypes.Role, AppRoles.OrganizationMember)
                                }));
                        }
                        
                        string googleId = ctx.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                        
                        
                        return Task.CompletedTask;
                    };
                });
        }
        
        public static bool IsTest(this IHostingEnvironment environment)
        {
            return environment.IsEnvironment(EnvironmentName.Test);
        }
    }
}