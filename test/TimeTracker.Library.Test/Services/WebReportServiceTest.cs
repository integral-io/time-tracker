using System;
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
    public class WebReportServiceTest : IAsyncLifetime
    {
        private readonly WebReportService webReportService;
        private readonly TimeTrackerDbContext database;
        private readonly Guid userId;
        private readonly DateTime date = new DateTime(DateTime.UtcNow.Date.Year, 2, 15);


        public WebReportServiceTest()
        {
            InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase = new InMemoryDatabaseWithProjectsAndUsers();
            database = inMemoryDatabase.Database;
            webReportService = new WebReportService(database);
            userId = database.Users.First(x => x.UserId != null).UserId;
        }

        public async Task InitializeAsync()
        {
            await PopulateTimeEntries();
        }

        [Fact]
        public async Task GetUserReport_onlyAccessesOneUserForAllEntriesAndCompressesByDay()
        {
            var report = await webReportService.GetUserReport(userId);
            report.Count.Should().Be(4);
            report.Where(x => x.UserId == userId).ToList().Count.Should().Be(4);
        }
        
        [Fact]
        public async Task GetUserReport_sumsUpHoursCorrectly()
        {
            var report = await webReportService.GetUserReport(userId);
            report.Count.Should().Be(4);
            
            report.Where(x => x.Date == date.ToShortDateString()).Sum(item => item.BillableHours).Should().Be(8);
            
            report.Where(x => x.Date == date.AddDays(-2).ToShortDateString()).Sum(item => item.BillableHours).Should().Be(6);
            report.Where(x => x.Date == date.AddDays(-2).ToShortDateString()).Sum(item => item.SickHours).Should().Be(2); 
            
            report.Where(x => x.Date == date.AddDays(-5).ToShortDateString()).Sum(item => item.BillableHours).Should().Be(4);
            report.Where(x => x.Date == date.AddDays(-5).ToShortDateString()).Sum(item => item.VacationHours).Should().Be(4);
            
            report.Where(x => x.Date == date.AddDays(-7).ToShortDateString()).Sum(item => item.OtherNonBillable).Should().Be(7);
        }

        private async Task PopulateTimeEntries()
        {
            var timeEntryService = new TimeEntryService(userId, database);

            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);
           
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-5), 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-5), 4, null, TimeEntryTypeEnum.Vacation);

            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-2), 4, 1, 1);
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-2), 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-2), 2, "dr visit", TimeEntryTypeEnum.Sick);


            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-7), 4, "pda", TimeEntryTypeEnum.NonBillable);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-7), 3, "lunch and learn", TimeEntryTypeEnum.NonBillable);

            var randomGuid = Guid.NewGuid();
            timeEntryService = new TimeEntryService(randomGuid, database);
            await timeEntryService.CreateBillableTimeEntry(date, 8, 1, 1);
        }

        [Fact]
        public async Task GetUserReport_listsEntriesInDescOrder()
        {
            var report = await webReportService.GetUserReport(userId);
            var expectedOrderReport = report.OrderByDescending(x => x.Date);
            Assert.True(expectedOrderReport.SequenceEqual(report));
        }

        public async Task DisposeAsync()
        {
            database?.Dispose();
        }
    }
}