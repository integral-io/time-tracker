using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Api.Test
{
    public static class TimeTrackerDbContextTestExtensions
    {
        public static Task<int> AddAutonomicAsBillingClientAndProject(this TimeTrackerDbContext db)
        {
            db.Database.EnsureCreated();
            db.BillingClients.Add(new BillingClient()
            {
                BillingClientId = 1,
                Name = "Autonomic"
            });
            db.Projects.Add(new Project()
            {
                ProjectId = 1,
                BillingClientId = 1,
                Name = "au"
            });
            return db.SaveChangesAsync();
        }
    }
}