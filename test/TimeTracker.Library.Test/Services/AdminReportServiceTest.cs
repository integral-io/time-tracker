using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Services;
using TimeTracker.TestInfra;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class AdminReportServiceTest : IAsyncLifetime 
    {
        private readonly AdminReportService adminReportService;

        private readonly TimeTrackerDbContext database;

        public AdminReportServiceTest()
        {
            InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase = new InMemoryDatabaseWithProjectsAndUsers();
            database = inMemoryDatabase.Database;
            adminReportService = new AdminReportService(database);
        }

        public async Task InitializeAsync()
        {
            await PopulateTimeEntries(1, new DateTime(DateTime.UtcNow.Date.Year, 2, 15));
        }

        [Fact]
        public async Task GetAllUsersReport_includesExpectedHours()
        {
            var report = await adminReportService.GetAllUsersReport();

            report.First().BillableHoursYtd.Should().Be(12);
            report.First().SickHoursYtd.Should().Be(6);
            report.First().VacationHoursYtd.Should().Be(13);
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

        [Fact]
        public async Task GetAllUsersByDateReport_includesExpectedHours()
        {
            var start1 = new DateTime(DateTime.UtcNow.Date.Year, 2, 7);
            var report = await adminReportService.GetAllUsersByDate(start1, start1.AddDays(14));

            report.First().BillableHoursYtd.Should().Be(12);
            report.First().SickHoursYtd.Should().Be(6);
            report.First().VacationHoursYtd.Should().Be(13);
            report.First().OtherNonBillableYtd.Should().Be(6);

            var start2 = new DateTime(DateTime.UtcNow.Date.Year, 2, 15);
            report = await adminReportService.GetAllUsersByDate(start2, start2.AddDays(14));

            report.First().BillableHoursYtd.Should().Be(8);
            report.First().SickHoursYtd.Should().Be(0);
            report.First().VacationHoursYtd.Should().Be(0);
            report.First().OtherNonBillableYtd.Should().Be(0);

        }

        [Fact]
        public async Task GetAllUsersByDateReport_filtersCustomRange()
        {
            var report = await adminReportService.GetAllUsersByDate(
                new DateTime(DateTime.UtcNow.Date.Year, 2, 7),
                new DateTime(DateTime.UtcNow.Date.Year, 2, 12));

            report.First().BillableHoursYtd.Should().Be(0);
            report.First().SickHoursYtd.Should().Be(4);
            report.First().VacationHoursYtd.Should().Be(13);
            report.First().OtherNonBillableYtd.Should().Be(6);
        }
        
        [Fact]
        public async Task GetAllUsersByDateReport_filtersProjectHours_andOnlyShowsBillableHours_whenProjectId()
        {
            await PopulateTimeEntries(2, new DateTime(DateTime.UtcNow.Date.Year, 3, 15));
            var report = await adminReportService.GetAllUsersByDate(
                new DateTime(DateTime.UtcNow.Date.Year, 2, 1),
                new DateTime(DateTime.UtcNow.Date.Year, 3, 30), 2);

            report.First().BillableHoursYtd.Should().Be(12);
            report.First().SickHoursYtd.Should().Be(0);
            report.First().VacationHoursYtd.Should().Be(0);
            report.First().OtherNonBillableYtd.Should().Be(0);
        }
        
        [Fact]
        public async Task GetAllUsersByDateReport_filtersUsersWithHoursForDates_whenFilterByProject()
        {
            var baseDate = new DateTime(DateTime.UtcNow.Date.Year, 3, 15);
            await PopulateTimeEntries(2, baseDate);
            var timeEntryService = new TimeEntryService(Guid.NewGuid(), database);
            await timeEntryService.CreateBillableTimeEntry(baseDate, 8, 1, 1);
            var report = await adminReportService.GetAllUsersByDate(
                new DateTime(DateTime.UtcNow.Date.Year, 2, 1),
                new DateTime(DateTime.UtcNow.Date.Year, 3, 30), 2);

            report.Should().HaveCount(1);
            report.First().BillableHoursYtd.Should().Be(12);
        }

        private async Task PopulateTimeEntries(int billableClientId, DateTime baseDate)
        {
            var timeEntryService = new TimeEntryService(database.Users.First().UserId, database);

            await timeEntryService.CreateBillableTimeEntry(baseDate, 8, billableClientId, billableClientId);
            await timeEntryService.CreateBillableTimeEntry(baseDate.AddDays(-billableClientId), 4, billableClientId, billableClientId);
            await timeEntryService.CreateNonBillableTimeEntry(baseDate.AddDays(-2), 2, "dr visit", TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateNonBillableTimeEntry(baseDate.AddDays(-3), 4, "dr visit", TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateNonBillableTimeEntry(baseDate.AddDays(-5), 7, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(baseDate.AddDays(-6), 6, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(baseDate.AddDays(-7), 4, "pda", TimeEntryTypeEnum.NonBillable);
            await timeEntryService.CreateNonBillableTimeEntry(baseDate.AddDays(-8), 2, "ipa", TimeEntryTypeEnum.NonBillable);
        }

        public async Task DisposeAsync()
        {
            database?.Dispose();
        }

    }
}