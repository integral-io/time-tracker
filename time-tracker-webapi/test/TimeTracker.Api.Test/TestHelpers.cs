using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;

namespace TimeTracker.Api.Test
{
    public class TestHelpers
    {
        public static DbContextOptions<TimeTrackerDbContext> BuildInMemoryDatabaseOptions(string dbName)
        {
            var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return options;
        }
    }
}