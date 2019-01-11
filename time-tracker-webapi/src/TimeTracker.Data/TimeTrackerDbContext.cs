using System;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Data
{
    public class TimeTrackerDbContext : DbContext
    {
        private readonly string _connectionString;

        protected TimeTrackerDbContext(string connectionString)
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

        public DbSet<Models.TimeEntry> TimeEntries { get; set; }
        public DbSet<Models.BillingClient> BillingClients { get; set; }
    }
}
