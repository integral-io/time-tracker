using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace TimeTracker.Api
{
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
                });
        }
        
        public static bool IsTest(this IHostingEnvironment environment)
        {
            return environment.IsEnvironment(EnvironmentName.Test);
        }
    }
}