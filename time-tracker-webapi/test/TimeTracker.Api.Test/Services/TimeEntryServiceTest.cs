using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Models;
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

        [Fact]
        public async Task QueryHours_pullsCorrectHoursAndInfo_forUser()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("hoursReport");
            
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(options))
            {
                TestHelpers.AddClientAndProject(context);
                
                TimeEntryService sut = new TimeEntryService(userId, context);
                DateTime dateBefore = new DateTime(2018,11,30);
                DateTime dateAfter = new DateTime(2019, 1, 15);
                await sut.CreateBillableTimeEntry(dateBefore, 7, 1, 1);
                await sut.CreateBillableTimeEntry(dateAfter, 7, 1, 1);

                var hours = await sut.QueryHours(new DateTime(2019, 1, 1));

                hours.ProjectHours.Count.Should().Be(1);
                hours.ProjectHours.Sum(x=>x.Hours).Should().Be(7);
                hours.ProjectHours.FirstOrDefault().TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);
                hours.ProjectHours.Select(x => x.ProjectOrName).Should().Contain("au");
            }
        }
        
        
        [Fact]
        public async Task AdminReport_GetsTimeOff_ForAllUsers()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("adminReport");
            
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(options))
            {
                TestHelpers.AddClientAndProject(context);
                TestHelpers.AddTestUsers(context);
                TestHelpers.AddTimeOff(context);
                
                TimeEntryService sut = new TimeEntryService(userId, context);

                AllTimeOffDto hours = await sut.QueryAllTimeOff();

                hours.TimeOffSummaries.Count.Should().Be(2);
                hours.TimeOffSummaries.FirstOrDefault().Username.Should().Be("username1");
                hours.TimeOffSummaries.LastOrDefault().PtoTYD.Should().Be(8);
                hours.TimeOffSummaries.FirstOrDefault().SickYTD.Should().Be(2);
            }
        }
        
        
    }
}