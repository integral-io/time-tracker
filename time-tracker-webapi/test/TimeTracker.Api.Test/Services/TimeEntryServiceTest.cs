using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Services;
using TimeTracker.Data;
using TimeTracker.Data.Models;
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
                entry.TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);
            }
        }

        [Fact]
        public async Task CreateNonBillableTimeEntry_createsDbRecord()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("projects");
            Guid userId = Guid.NewGuid();

            using (var context = new TimeTrackerDbContext(options))
            {
                TimeEntryService sut = new TimeEntryService(userId, context);
                DateTime date = DateTime.UtcNow.Date;
                string nonBillReason = "sick with flu";
                Guid id = await sut.CreateNonBillableTimeEntry(date, 6, nonBillReason, 
                    TimeEntryTypeEnum.Sick);

                var entry = await context.TimeEntries.FirstOrDefaultAsync(x => x.TimeEntryId == id);
                entry.Should().NotBeNull();
                entry.Hours.Should().Be(6);
                entry.TimeEntryType.Should().Be(TimeEntryTypeEnum.Sick);
                entry.NonBillableReason.Should().Be(nonBillReason);
            }
        }

        [Fact]
        public async Task DeleteHours_ThatDontExistFromToday_works()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("hoursDeleted1");
            Guid userId = Guid.NewGuid();

            using (var context = new TimeTrackerDbContext(options))
            {
                TimeEntryService sut = new TimeEntryService(userId, context);
                double hoursDeleted = await sut.DeleteHours(DateTime.UtcNow);

                hoursDeleted.Should().Be(0);
            }
        }

        [Fact]
        public async Task DeleteHours_ThatDoExistToday_works()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("hoursDeleted1");
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(options))
            {
                TimeEntryService sut = new TimeEntryService(userId, context);
                DateTime date = DateTime.UtcNow.Date;
                Guid id = await sut.CreateBillableTimeEntry(date, 7, 1, 1);

                var entry = await context.TimeEntries.FirstOrDefaultAsync(x => x.TimeEntryId == id);
                double hoursDeleted = await sut.DeleteHours(date);

                hoursDeleted.Should().Be(7);
            }
        }
    }
}