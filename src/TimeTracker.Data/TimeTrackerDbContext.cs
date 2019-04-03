using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Data
{
    public class TimeTrackerDbContext : DbContext
    {
        public TimeTrackerDbContext(DbContextOptions options) : base(options)
        {
        }

        public void DetachEntity(object entity)
        {
            Entry(entity).State = EntityState.Detached;
        }

        public DbSet<Models.BillingClient> BillingClients { get; set; }
        public DbSet<Models.BillingRate> BillingRates { get; set; }
        public DbSet<Models.Project> Projects { get; set; }
        public DbSet<Models.TimeEntry> TimeEntries { get; set; }
        public DbSet<Models.User> Users { get; set; }
    }
}
