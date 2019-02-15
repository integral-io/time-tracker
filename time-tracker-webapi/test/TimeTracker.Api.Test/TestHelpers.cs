using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Api.Test
{
    public static class TestHelpers
    {
        public static void InitializeDatabaseForTests(TimeTrackerDbContext db)
        {
            AddClientAndProject(db);
        }

        public static void AddClientAndProject(TimeTrackerDbContext dbContext)
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