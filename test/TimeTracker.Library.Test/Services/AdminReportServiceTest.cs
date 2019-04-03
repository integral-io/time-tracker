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
    public class AdminReportServiceTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>, IAsyncLifetime
    {
        private readonly TimeTrackerDbContext database;
        private readonly AdminReportService adminReportService;

        public AdminReportServiceTest(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            database = inMemoryDatabase.Database;
            adminReportService = new AdminReportService(database);
        }

        public async Task InitializeAsync()
        {
            await PopulateTimeEntries();
        }

        [Fact]
        public async Task GetAllUsersReport_includesExpectedHours()
        {
            var report = await adminReportService.GetAllUsersReport();

            report.First().BillableHoursYtd.Should().Be(12);
            report.First().SickHoursYtd.Should().Be(6);
            report.First().VacationHoursYtd.Should().Be(14);
            report.First().OtherNonBillableYtd.Should().Be(6);
        }

        [Fact]
        public async Task GetAllUsersReport_includesExpectedUsers()
        {
            var report = await adminReportService.GetAllUsersReport();
            report.Count.Should().Be(2);

            report.Where(x => x.SlackUserName == database.Users.First().UserName).Should().NotBeEmpty();
            report.Where(x => x.SlackUserName == database.Users.Last().UserName).Should().NotBeEmpty();
        }

        private async Task PopulateTimeEntries()
        {
            var timeEntryService = new TimeEntryService(database.Users.First().UserId, database);
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

        public async Task DisposeAsync()
        {
        }
    }
}