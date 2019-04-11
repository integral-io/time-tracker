using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Services;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class TimeEntryServiceTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>
    {
        private readonly Guid userId = Guid.NewGuid();

        private readonly TimeTrackerDbContext database;
        private readonly TimeEntryService timeEntryService;

        public TimeEntryServiceTest()
        {
            var inMemoryDatabase = new InMemoryDatabaseWithProjectsAndUsers();
            database = inMemoryDatabase.Database;
            timeEntryService = new TimeEntryService(userId, database);
        }

        [Fact]
        public async Task CreateBillableTimeEntry_createsDbRecord()
        {
            await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow.Date,
                7, 1, 1);

            var entry = await database.TimeEntries.FirstAsync(x => x.UserId == userId);
            entry.Hours.Should().Be(7);
            entry.TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);
        }

        [Fact]
        public async Task CreateNonBillableTimeEntry_createsDbRecord()
        {
            var nonBillReason = "sick with flu";

            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow.Date,
                6, nonBillReason, TimeEntryTypeEnum.Sick);

            var entry = await database.TimeEntries.FirstAsync(x => x.UserId == userId);
            entry.Hours.Should().Be(6);
            entry.TimeEntryType.Should().Be(TimeEntryTypeEnum.Sick);
            entry.NonBillableReason.Should().Be(nonBillReason);
        }

        [Fact]
        public async Task DeleteHours_ThatDontExistFromToday_works()
        {
            var hoursDeleted = await timeEntryService.DeleteHours(DateTime.UtcNow);

            hoursDeleted.Should().Be(0);
        }

        [Fact]
        public async Task DeleteHours_ThatDoExistToday_works()
        {
            var date = DateTime.UtcNow.Date;
            await timeEntryService.CreateBillableTimeEntry(date, 7, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date, 8, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date, 8, "flu", TimeEntryTypeEnum.Sick);

            var hoursDeleted = await timeEntryService.DeleteHours(date);

            hoursDeleted.Should().Be(23);
        }

        [Fact]
        public async Task AdminReport_GetsTimeOff_ForAllUsers()
        {
            database.AddTimeOff();

            var hours = await timeEntryService.QueryAllTimeOff();

            hours.TimeOffSummaries.Count.Should().Be(2);
            hours.TimeOffSummaries.First().Username.Should().Be("username1");
            hours.TimeOffSummaries.Last().PtoTyd.Should().Be(8);
            hours.TimeOffSummaries.First().SickYtd.Should().Be(2);
        }

        [Fact]
        public async Task HoursCannotBeDeleted48HoursPastEntry_AndExceptionWillBeThrown()
        {
            var date = DateTime.UtcNow.Date.AddHours(-48);
            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);

            var lateDate = DateTime.UtcNow.Date.AddHours(-48.01);
            await timeEntryService.CreateBillableTimeEntry(lateDate, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(lateDate, 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(lateDate, 1, "flu", TimeEntryTypeEnum.Sick);

            try
            {
                await Assert.ThrowsAsync<Exception>(() => timeEntryService.DeleteHours(lateDate));
            }
            catch (Exception e)
            {
                Assert.Equal("Entries older than 48 hours cannot be deleted.", e.Message);
            }

            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(14);

            var hoursDeleted = await timeEntryService.DeleteHours(date);
            timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(6);
            hoursDeleted.Should().Be(8);
        }

        [Fact]
        public async Task WhenDeletingHoursForADay_AllHoursOnThatDateAreDeleted()
        {
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);
            hoursLeft.Should().Be(0);
            
            var date = DateTime.UtcNow.AddHours(-2);
            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);
            
            var hoursDeleted = await timeEntryService.DeleteHours(DateTime.UtcNow.Date);
            timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(0);
            hoursDeleted.Should().Be(8);
            
        }
    }
}