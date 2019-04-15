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
    public class WebReportServiceTest : IAsyncLifetime
    {
        private readonly WebReportService webReportService;
        private readonly TimeTrackerDbContext database;
        private readonly Guid userId = Guid.NewGuid();

        public WebReportServiceTest()
        {
            InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase = new InMemoryDatabaseWithProjectsAndUsers();
            database = inMemoryDatabase.Database;
            webReportService = new WebReportService(database);
        }

        public async Task InitializeAsync()
        {
            await PopulateTimeEntries();
        }

        [Fact]
        public async Task GetUserReport_onlyAccessesOneUserForAllEntries()
        {
            var report = await webReportService.GetUserReport(userId);
            report.Count.Should().Be(4);
            report.Where(x => x.UserId == userId).ToList().Count.Should().Be(4);
        }

        private async Task PopulateTimeEntries()
        {
            var timeEntryService = new TimeEntryService(userId, database);
            var date = new DateTime(DateTime.UtcNow.Date.Year, 2, 15);

            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-2), 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-2), 2, "dr visit", TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-5), 4, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-5), 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-7), 4, "pda", TimeEntryTypeEnum.NonBillable);

            var randomGuid = Guid.NewGuid();
            timeEntryService = new TimeEntryService(randomGuid, database);
            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);
        }

        public async Task DisposeAsync()
        {
            database?.Dispose();
        }
    }
}