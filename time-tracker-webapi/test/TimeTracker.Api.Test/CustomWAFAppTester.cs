using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Data;

namespace TimeTracker.Api.Test
{
    public class CustomWAFAppTester<TStartup> : WebApplicationFactory<TStartup> where TStartup: class
    {
        public TimeTrackerDbContext DbContext { get; set; }
        public IServiceScope ServiceScope { get; set; }
        
        /// <summary>
        /// Configures DI to use in memory database for controllers, and sets the DbContext and ServiceScope for
        /// later use by tests to validate if entries made it to DB etc. Always dispose ServiceScope at end of test
        /// </summary>
        /// <param name="builder"></param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Create a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add a database context (ApplicationDbContext) using an in-memory 
                // database for testing.
                services.AddDbContext<TimeTrackerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (ApplicationDbContext).
                ServiceScope = sp.CreateScope();

                var scopedServices = ServiceScope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TimeTrackerDbContext>();

                var logger = scopedServices
                    .GetRequiredService<ILogger<CustomWAFAppTester<TStartup>>>();

                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data.
                    TestHelpers.InitializeDatabaseForTests(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred seeding the " +
                                        "database with test messages. Error: {ex.Message}");
                }

                this.DbContext = db;

            });
        }
    }
}