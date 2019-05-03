using System;
using TimeTracker.Data;

namespace TimeTracker.TestInfra
{
    public class InMemoryDatabaseWithProjectsAndUsers : IDisposable
    {
        public TimeTrackerDbContext Database { get; }

        public InMemoryDatabaseWithProjectsAndUsers()
        {
            Database = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions(Guid.NewGuid().ToString()));
            
            Database.AddAutonomicAsClientAndProject();
            Database.AddTestUsers();
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}