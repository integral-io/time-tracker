using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Services;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class UserReportServiceTest
    {
        [Fact]
        public async Task QueryHours_pullsCorrectHoursAndInfo_forUser()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("hoursReport");
            
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(options))
            {
                TestHelpers.AddClientAndProject(context);
                
                TimeEntryService timeEntryService = new TimeEntryService(userId, context);
                DateTime dateBefore = new DateTime(2018,11,30);
                DateTime dateAfter = new DateTime(2019, 1, 15);
                await timeEntryService.CreateBillableTimeEntry(dateBefore, 7, 1, 1);
                await timeEntryService.CreateBillableTimeEntry(dateAfter, 7, 1, 1);

                UserReportService sut = new UserReportService(context, userId);
                var hours = await sut.QueryAllHours();

                hours.Count.Should().Be(2);
                hours.Sum(x=>x.Hours).Should().Be(14);
                hours.FirstOrDefault().TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);
                hours.Select(x => x.ProjectOrName).Should().Contain("au");
            }
        }

        [Fact]
        public async Task GetHoursSummary_summarizesHoursCorrectly()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("hoursReport");
            
            Guid userId = Guid.NewGuid();
            using (var context = new TimeTrackerDbContext(options))
            {
                TestHelpers.AddClientAndProject(context);

                TimeEntryService timeEntryService = new TimeEntryService(userId, context);
                DateTime dateBefore = new DateTime(2018, 11, 30);
                DateTime dateAfter = new DateTime(2019, 1, 15);
                await timeEntryService.CreateBillableTimeEntry(dateBefore, 7, 1, 1);
                await timeEntryService.CreateBillableTimeEntry(dateAfter, 7, 1, 1);

                UserReportService sut = new UserReportService(context, userId);
                var hours = await sut.GetHoursSummary();
                
                // todo: fill in

                hours.currentMonthDisplay.Should().Be("March 2019");
                hours.billableHoursMonth.Should().Be(7d);
                hours.billableHoursYtd.Should().Be(7d);
                hours.sickHoursMonth.Should().Be(7d);
                hours.vacationHoursMonth.Should().Be(7d);
                hours.nonBillableHoursMonth.Should().Be(7d);
                hours.sickHoursYTD.Should().Be(7d);
                hours.vacationHoursYTD.Should().Be(7d);
                hours.nonBillableHoursYTD.Should().Be(7d);
            }
        }
    }
}