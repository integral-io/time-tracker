using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.TestInfra
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

        public static void AddAutonomicAsClientAndProject(this TimeTrackerDbContext dbContext)
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

        public static void AddTestUsers(this TimeTrackerDbContext dbContext)
        {
            dbContext.Users.AddRange(new User()
                {
                    UserId = Guid.NewGuid(),
                    LastName = "last1",
                    FirstName = "first1",
                    SlackUserId = "slackId1",
                    UserName = "username1",
                    GoogleIdentifier = Guid.NewGuid().ToString()
                }, new User()
                {
                    UserId = Guid.NewGuid(),
                    LastName = "last2",
                    FirstName = "first2",
                    SlackUserId = "slackId2",
                    UserName = "username2",
                    GoogleIdentifier = Guid.NewGuid().ToString()
                }
            );
            
            dbContext.SaveChanges();
        }

        public static void AddTimeOff(this TimeTrackerDbContext dbContext)
        {
            dbContext.TimeEntries.AddRange(
                new TimeEntry()
                {
                    Date = new DateTime(2018,12,1),
                    IsBillable = false,
                    Hours = 2,
                    TimeEntryId = Guid.NewGuid(),
                    TimeEntryType = TimeEntryTypeEnum.Sick,
                    UserId = dbContext.Users.First().UserId,
                    NonBillableReason = "sick"
                },
                new TimeEntry()
                {
                    Date = new DateTime(2018,12,2),
                    IsBillable = false,
                    Hours = 8,
                    TimeEntryId = Guid.NewGuid(),
                    TimeEntryType = TimeEntryTypeEnum.Vacation,
                    UserId = dbContext.Users.Last().UserId,
                    NonBillableReason = "sick2"
                }
                );

            dbContext.SaveChanges();
        }
    }
}