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
            var options = TestHelpers.BuildInMemoryDatabaseOptions("queryHours1");
            
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
                int currentYear = DateTime.UtcNow.Year;
                int testMonth = 3;

                TimeEntryService timeEntryService = new TimeEntryService(userId, context);
                DateTime dateBefore = new DateTime(2018, 11, 30);
                DateTime dateAfter = new DateTime(currentYear, testMonth, 2);
                await timeEntryService.CreateBillableTimeEntry(dateBefore, 7, 1, 1);
                await timeEntryService.CreateNonBillableTimeEntry(dateBefore.AddDays(-1), 5, null,
                    TimeEntryTypeEnum.Vacation);
                
                await timeEntryService.CreateBillableTimeEntry(dateAfter, 4, 1, 1);
                await timeEntryService.CreateBillableTimeEntry(dateAfter.AddDays(1), 8, 1, 1);
                await timeEntryService.CreateBillableTimeEntry(dateAfter.AddDays(2), 8, 1, 1);
                await timeEntryService.CreateBillableTimeEntry(dateAfter.AddMonths(-1), 8, 1, 1);
                
                await timeEntryService.CreateNonBillableTimeEntry(dateAfter.AddDays(15), 5, null,
                    TimeEntryTypeEnum.Vacation);
                await timeEntryService.CreateNonBillableTimeEntry(dateAfter.AddDays(16), 6, "dr visit",
                    TimeEntryTypeEnum.Sick);
                await timeEntryService.CreateNonBillableTimeEntry(dateAfter.AddDays(17), 7, "PDA",
                    TimeEntryTypeEnum.NonBillable);

                UserReportService sut = new UserReportService(context, userId);
                var hours = await sut.GetHoursSummaryMonthAndYtd(testMonth);
                
                hours.CurrentMonthDisplay.Should().Be("March 2019");
                hours.BillableHoursMonth.Should().Be(20d);
                hours.BillableHourssYtd.Should().Be(28d);
                
                hours.SickHoursMonth.Should().Be(6d);
                hours.VacationHoursMonth.Should().Be(5d);
                hours.NonBillableHoursMonth.Should().Be(7d);
                
                hours.SickHoursYtd.Should().Be(6d);
                hours.VacationHoursYtd.Should().Be(5d);
                hours.NonBillableHoursYtd.Should().Be(7d);
            }
        }
    }
}