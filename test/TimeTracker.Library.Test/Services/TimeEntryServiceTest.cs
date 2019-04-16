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
    public class TimeEntryServiceTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>, IDisposable
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
        public async Task EachEntryMustBeGreaterThanZeroHours()
        {
            try
            {
                await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow.Date, 0, 1, 1);
            }
            catch (Exception e)
            {
                Assert.Equal("An entry should have more than 0 hours.", e.Message);
            }

            try
            {
                await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow.Date, 0, null,
                    TimeEntryTypeEnum.Vacation);
            }
            catch (Exception e)
            {
                Assert.Equal("An entry should have more than 0 hours.", e.Message);
            }
        }

        [Fact]
        public async Task TheEntriesForADayShouldBeNoGreaterThan24Hours_WhenAddingNonBillableTimeOver()
        {
            await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow, 8, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 8, null, TimeEntryTypeEnum.Vacation);

            await Assert.ThrowsAsync<Exception>(() =>
                timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 8.5, "flu", TimeEntryTypeEnum.Sick));
            try
            {
                await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 8.5, "flu", TimeEntryTypeEnum.Sick);
            }
            catch (Exception e)
            {
                Assert.Equal("You may not enter more than 24 hours per day.", e.Message);
            }
            
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hours = timeEntries.Sum(x => x.Hours);
            hours.Should().Be(16);
        }
        
        [Fact]
        public async Task TheEntriesForADayShouldBeNoGreaterThan24Hours_WhenAddingBillableTimeOver()
        {
            await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow, 8, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 8, null, TimeEntryTypeEnum.Vacation);

            await Assert.ThrowsAsync<Exception>(() =>
                timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow, 8.5, 1, 1));
            try
            {
                await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow, 8.5, 1, 1);;
            }
            catch (Exception e)
            {
                Assert.Equal("You may not enter more than 24 hours per day.", e.Message);
            }
            
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hours = timeEntries.Sum(x => x.Hours);
            hours.Should().Be(16);
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

            var hoursDeleted = await timeEntryService.DeleteHours(date);

            hoursDeleted.Should().Be(15);

            await timeEntryService.CreateNonBillableTimeEntry(date, 8, "flu", TimeEntryTypeEnum.Sick);

            hoursDeleted = await timeEntryService.DeleteHours(date);

            hoursDeleted.Should().Be(8);

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
                await timeEntryService.DeleteHours(lateDate);
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
            
            var hoursDeleted = await timeEntryService.DeleteHours(date.Date);
            timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(0);
            hoursDeleted.Should().Be(8);
        }
        
        [Fact]
        public async Task WhenDeletingBillableHoursOnADay_AllBillableHoursOnThatDayAreDeleted()
        {
            var date = DateTime.UtcNow.AddHours(-2);
            await timeEntryService.CreateBillableTimeEntry(date, 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 3, null, TimeEntryTypeEnum.Vacation);
            
            var hoursDeleted = await timeEntryService.DeleteHoursForTimeEntryType(date.Date, TimeEntryTypeEnum.BillableProject);
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(3);
            hoursDeleted.Should().Be(4);
        }
        
        [Fact]
        public async Task WhenDeletingVacationHoursOnADay_AllVacationHoursOnThatDayAreDeleted()
        {
            var date = DateTime.UtcNow.AddHours(-2);
            await timeEntryService.CreateBillableTimeEntry(date, 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 3, null, TimeEntryTypeEnum.Vacation);
            
            var hoursDeleted = await timeEntryService.DeleteHoursForTimeEntryType(date.Date, TimeEntryTypeEnum.Vacation);
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(4);
            hoursDeleted.Should().Be(3);
        }
        
        [Fact]
        public async Task WhenDeletingSickHoursOnADay_AllSickHoursOnThatDayAreDeleted()
        {
            var date = DateTime.UtcNow.AddHours(-2);
            await timeEntryService.CreateBillableTimeEntry(date, 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 5, "flu", TimeEntryTypeEnum.Sick);
            
            var hoursDeleted = await timeEntryService.DeleteHoursForTimeEntryType(date.Date, TimeEntryTypeEnum.Sick);
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(4);
            hoursDeleted.Should().Be(5);
        }

        [Fact] public async Task WhenDeletingNonBillableHoursOnADay_AllNonBillableHoursOnThatDayAreDeleted()
        {
            var date = DateTime.UtcNow.AddHours(-2);
            await timeEntryService.CreateBillableTimeEntry(date, 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 2, "beach", TimeEntryTypeEnum.NonBillable);
            
            var hoursDeleted = await timeEntryService.DeleteHoursForTimeEntryType(date.Date, TimeEntryTypeEnum.NonBillable);
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(4);
            hoursDeleted.Should().Be(2);
        }
        
        [Fact]
        public async Task HoursForASpecificTypeCannotBeDeleted48HoursPastEntry_AndExceptionWillBeThrown()
        {
            var date = DateTime.UtcNow.Date.AddHours(-48);
            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);

            var lateDate = DateTime.UtcNow.Date.AddHours(-48.01);
            await timeEntryService.CreateBillableTimeEntry(lateDate, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(lateDate, 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(lateDate, 1, "flu", TimeEntryTypeEnum.Sick);

            try
            {
                await timeEntryService.DeleteHoursForTimeEntryType(lateDate, TimeEntryTypeEnum.BillableProject);
            }
            catch (Exception e)
            {
                Assert.Equal("BillableProject time entries older than 48 hours cannot be deleted.", e.Message);
            }

            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(14);

            var hoursDeleted = await timeEntryService.DeleteHoursForTimeEntryType(date, TimeEntryTypeEnum.BillableProject);
            timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            hoursLeft = timeEntries.Sum(x => x.Hours);

            hoursLeft.Should().Be(6);
            hoursDeleted.Should().Be(8);
        }

        public void Dispose()
        {
            database?.Dispose();
        }

        [Fact]
        public async Task WhenAddingSickAndVacationTime_CannotAddMoreThan8HoursPerDay()
        {
            await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 5, null, TimeEntryTypeEnum.Vacation);
            await Assert.ThrowsAsync<Exception>(() => timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 4, "flu", TimeEntryTypeEnum.Sick));
            try
            {
                await timeEntryService.CreateNonBillableTimeEntry(DateTime.UtcNow, 4, "flu", TimeEntryTypeEnum.Sick);
            }
            catch (Exception e)
            {
                Assert.Equal("Cannot have more than 8 hours of combined vacation and sick time in a single day.", e.Message);
            }
            
            var timeEntries = await database.TimeEntries.Where(x => x.UserId == userId).ToListAsync();
            var hours = timeEntries.Sum(x => x.Hours);
            hours.Should().Be(5);
        }
    }
}