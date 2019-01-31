using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Api.Test
{
    public static class TestHelpers
    {
        public static DbContextOptions<TimeTrackerDbContext> BuildInMemoryDatabaseOptions(string dbName)
        {
            var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return options;
        }

        public static void InitializeDatabaseForTests(TimeTrackerDbContext db)
        {
            AddClientAndProject(db);
        }
        
        private static void AddClientAndProject(TimeTrackerDbContext dbContext)
        {
            dbContext.BillingClients.Add(new BillingClient()
            {
                BillingClientId = 1,
                Name = "Autonomic"
            });
            dbContext.Projects.Add(new Project()
            {
                ProjectId = 1,
                BillingClientId = 1,
                Name = "au"
            });
            dbContext.SaveChanges();
        }
    }
}