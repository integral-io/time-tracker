using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Services;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class AdminReportServiceTest
    {
        [Fact]
        public async Task GetAllUsersReport_includesExpectedHours()
        {
            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("adminReportExpectedHours")))
            {
                TestHelpers.AddClientAndProject(context);
                var testUsers = TestHelpers.AddTestUsers(context);
                await PopulateTimeEntries(context, testUsers);
                
                var sut = new AdminReportService(context);
                var report = await sut.GetAllUsersReport();

                report.First().BillableHoursYtd.Should().Be(12);
                report.First().SickHoursYtd.Should().Be(6);
                report.First().VacationHoursYtd.Should().Be(14);
                report.First().OtherNonBillableYtd.Should().Be(6);
            }
        }
        
        [Fact]
        public async Task GetAllUsersReport_includesExpectedUsers()
        {
            using (var context = new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions("adminReportExpectedUsers")))
            {
                TestHelpers.AddClientAndProject(context);
                var testUsers = TestHelpers.AddTestUsers(context);
                await PopulateTimeEntries(context, testUsers);
                
                var sut = new AdminReportService(context);
                var report = await sut.GetAllUsersReport();
                report.Count.Should().Be(2);

                report.Where(x => x.SlackUserName == testUsers.First().UserName).Should().NotBeEmpty();
                report.Where(x => x.SlackUserName == testUsers.Last().UserName).Should().NotBeEmpty();
            }
        }

        private async Task PopulateTimeEntries(TimeTrackerDbContext context, IEnumerable<User> testUsers)
        {
            var timeEntryService = new TimeEntryService(testUsers.First().UserId, context);
            var date = new DateTime(DateTime.UtcNow.Date.Year, 2, 15);

            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-1), 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-3), 4, "dr visit",
                TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-2), 2, "dr visit",
                TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-5), 8, null,
                TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-6), 6, null,
                TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-7), 4, "pda",
                TimeEntryTypeEnum.NonBillable);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-8), 2, "ipa",
                TimeEntryTypeEnum.NonBillable);
        }
    }
}