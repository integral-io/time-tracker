using System;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Data
{
    public class TimeTrackerDbContext : DbContext
    {
        private readonly string _connectionString;

        public TimeTrackerDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }
        
        public TimeTrackerDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Models.BillingClient> BillingClients { get; set; }
        public DbSet<Models.BillingRate> BillingRates { get; set; }
        public DbSet<Models.Project> Projects { get; set; }
        public DbSet<Models.TimeEntry> TimeEntries { get; set; }
        public DbSet<Models.User> Users { get; set; }
    }
}
