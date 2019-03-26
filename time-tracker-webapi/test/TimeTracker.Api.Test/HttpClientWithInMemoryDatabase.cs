using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Data;

namespace TimeTracker.Api.Test
{
    public class HttpClientWithInMemoryDatabase
    {
        private readonly WebApplicationFactory<Startup> factory;
        private ServiceProvider serviceProvider;

        public HttpClientWithInMemoryDatabase()
        {
            factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(ConfigureWebHost);
        }

        public HttpClient Client => factory.CreateClient();

        private void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(async services =>
            {
                services.AddEntityFrameworkInMemoryDatabase();
                services.AddDbContext<TimeTrackerDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                });

                serviceProvider = services.BuildServiceProvider();

                var db = serviceProvider.GetRequiredService<TimeTrackerDbContext>();
                await db.AddAutonomicAsBillingClientAndProject();
            });
        }
    }
}