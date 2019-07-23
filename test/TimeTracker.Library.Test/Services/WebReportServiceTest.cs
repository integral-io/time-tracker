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
        private readonly DateTime defaultDate = new DateTime(DateTime.UtcNow.Date.Year, DateTime.UtcNow.Date.Month, 15);


        public WebReportServiceTest()
        {
            InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase = new InMemoryDatabaseWithProjectsAndUsers();
            database = inMemoryDatabase.Database;
            webReportService = new WebReportService(database);
            userId = database.Users.First(x => x.UserId != null).UserId;
        }

        /// <summary>
        /// called at the beginning of class instantiation. Kind of like @Before but support for async
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            await PopulateTimeEntries(defaultDate);
        }

        [Fact]
        public async Task GetAvailableMonths_getsMonths()
        {
            var months = await webReportService.GetUserAvailableMonths(userId);
            months.Should().NotBeEmpty();
            months.FirstOrDefault().Value.Should().Be($"{defaultDate.Year}-{defaultDate.Month}-01");
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
            
            report.Where(x => x.Date == defaultDate.ToShortDateString()).Sum(item => item.BillableHours).Should().Be(8);
            
            report.Where(x => x.Date == defaultDate.AddDays(-2).ToShortDateString()).Sum(item => item.BillableHours).Should().Be(6);
            report.Where(x => x.Date == defaultDate.AddDays(-2).ToShortDateString()).Sum(item => item.SickHours).Should().Be(2); 
            
            report.Where(x => x.Date == defaultDate.AddDays(-5).ToShortDateString()).Sum(item => item.BillableHours).Should().Be(4);
            report.Where(x => x.Date == defaultDate.AddDays(-5).ToShortDateString()).Sum(item => item.VacationHours).Should().Be(4);
            
            report.Where(x => x.Date == defaultDate.AddDays(-7).ToShortDateString()).Sum(item => item.OtherNonBillable).Should().Be(7);

            report.Where(x => x.Date == defaultDate.ToShortDateString()).Select(item => item.TotalHours).FirstOrDefault()
                .Should().Be(8);
        }

        [Fact]
        public async Task GetUserReport_returnsDataForSpecificMonth()
        {
            int expectedMonth = 4;
            DateTime aprilDate = new DateTime(DateTime.UtcNow.Date.Year, 4, 15);
            await PopulateTimeEntries(aprilDate);
            var report = await webReportService.GetUserReport(userId, expectedMonth);

            report.Where(x => x.DateForOrdering.Month.Equals(expectedMonth)).Should().NotBeEmpty();
            report.Where(x => x.DateForOrdering.Month.Equals(expectedMonth)).Should().HaveCount(4);
        }

        [Fact]
        public async Task GetUserReport_listsEntriesInDescOrder()
        {
            var report = await webReportService.GetUserReport(userId);
            var expectedOrderReport = report.OrderByDescending(x => x.DateForOrdering);
            Assert.True(expectedOrderReport.SequenceEqual(report));
        }

        private async Task PopulateTimeEntries(DateTime date)
        {
            var timeEntryService = new TimeEntryService(userId, database);

            await timeEntryService.CreateBillableTimeEntry(date, 8, 1);
           
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-5), 4, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-5), 4, null, TimeEntryTypeEnum.Vacation);

            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-2), 4, 1);
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-2), 2, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-2), 2, "dr visit", TimeEntryTypeEnum.Sick);


            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-7), 4, "pda", TimeEntryTypeEnum.NonBillable);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-7), 3, "lunch and learn", TimeEntryTypeEnum.NonBillable);

            var randomGuid = Guid.NewGuid();
            timeEntryService = new TimeEntryService(randomGuid, database);
            await timeEntryService.CreateBillableTimeEntry(date, 8, 1);
        }

        public async Task DisposeAsync()
        {
            database?.Dispose();
        }
    }
}