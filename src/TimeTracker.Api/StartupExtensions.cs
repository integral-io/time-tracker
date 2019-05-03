using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema.Infrastructure;
using TimeTracker.Data;

namespace TimeTracker.Api
{
    public static class AppRoles
    {
        public const string OrganizationMember = "organizationmember";
        public const string Admin = "admin";
    }
    public static class StartupExtensions
    {
        private const string OrganizationEmailConfigurationKey = "OrganizationDomain";
        
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
                        var rolesToAdd = new List<Claim>();
                        IConfiguration configuration = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                        if (configuration != null)
                        {
                            string organizationDomain = configuration.GetValue<string>(OrganizationEmailConfigurationKey);
                            if (!string.IsNullOrEmpty(organizationDomain) && email.EndsWith("@" + organizationDomain))
                            {
                                rolesToAdd.Add(new Claim(ClaimTypes.Role, AppRoles.OrganizationMember));
                            }
                        }

                        string googleId = ctx.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                        var db = ctx.HttpContext.RequestServices.GetRequiredService<TimeTrackerDbContext>();
                        var user = db.Users.FirstOrDefault(x => x.GoogleIdentifier == googleId);
                        if (user != null && user.Roles != null && user.Roles.Contains(AppRoles.Admin))
                        {
                            rolesToAdd.Add(new Claim(ClaimTypes.Role, AppRoles.Admin));
                        }

                        if (rolesToAdd.Count > 0)
                        {
                            ctx.Principal.AddIdentity(new ClaimsIdentity(rolesToAdd));
                        }
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