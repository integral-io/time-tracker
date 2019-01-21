using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Services;
using TimeTracker.Data;
using Xunit;

namespace TimeTracker.Api.Test.Services
{
    public class TimeEntryServiceTest
    {
        [Fact]
        public async Task CreateBillableTimeEntry_createsDbRecord()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("projects");
            Guid userId = Guid.NewGuid();

            using (var context = new TimeTrackerDbContext(options))
            {
                TimeEntryService sut = new TimeEntryService(userId, context);
                DateTime date = DateTime.UtcNow.Date;
                Guid id = await sut.CreateBillableTimeEntry(date, 7, 1, 1);

                var entry = await context.TimeEntries.FirstOrDefaultAsync(x => x.TimeEntryId == id);
                entry.Should().NotBeNull();
                entry.Hours.Should().Be(7);
            }
        }
    }
}