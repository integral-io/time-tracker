using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class TimeEntryServiceTest
    {
        [Fact]
        public async Task CreateBillableTimeEntry_createsDbRecord()
        {
            Guid userId = Guid.NewGuid();

            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("projects")))
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
            Guid userId = Guid.NewGuid();

            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("projects")))
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
            Guid userId = Guid.NewGuid();

            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("hoursDeleted1")))
            {
                TimeEntryService sut = new TimeEntryService(userId, context);
                double hoursDeleted = await sut.DeleteHours(DateTime.UtcNow);

                hoursDeleted.Should().Be(0);
            }
        }

        [Fact]
        public async Task DeleteHours_ThatDoExistToday_works()
        {
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("hoursDeleted2")))
            {
                TimeEntryService sut = new TimeEntryService(userId, context);
                DateTime date = DateTime.UtcNow.Date;
                Guid id = await sut.CreateBillableTimeEntry(date, 7, 1, 1);

                var entry = await context.TimeEntries.FirstOrDefaultAsync(x => x.TimeEntryId == id);
                double hoursDeleted = await sut.DeleteHours(date);

                hoursDeleted.Should().Be(7);
            }
        }

        [Fact]
        public async Task AdminReport_GetsTimeOff_ForAllUsers()
        {
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("obsoleteAdminReport")))
            {
                TestHelpers.AddClientAndProject(context);
                TestHelpers.AddTestUsers(context);
                TestHelpers.AddTimeOff(context);
                
                TimeEntryService sut = new TimeEntryService(userId, context);

                AllTimeOff hours = await sut.QueryAllTimeOff();

                hours.TimeOffSummaries.Count.Should().Be(2);
                hours.TimeOffSummaries.FirstOrDefault().Username.Should().Be("username1");
                hours.TimeOffSummaries.LastOrDefault().PtoTyd.Should().Be(8);
                hours.TimeOffSummaries.FirstOrDefault().SickYtd.Should().Be(2);
            }
        }
    }
}