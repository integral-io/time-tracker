using System;
using TimeTracker.Data;

namespace TimeTracker.Library.Test.Services
{
    public class InMemoryDatabase : IDisposable
    {
        public TimeTrackerDbContext Database { get; }

        public InMemoryDatabase()
        {
            Database = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions(Guid.NewGuid().ToString()));
            
            TestHelpers.AddClientAndProject(Database);
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}