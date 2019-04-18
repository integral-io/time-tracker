using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data.Models;
using TimeTracker.Library.Services;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class UserReportServiceTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>
    {
        private readonly Guid userId = Guid.NewGuid();
        
        private readonly TimeEntryService entryService;
        private readonly UserReportService userReportService;

        public UserReportServiceTest(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            entryService = new TimeEntryService(userId, inMemoryDatabase.Database);
            userReportService = new UserReportService(inMemoryDatabase.Database, userId);
        }

        [Fact]
        public async Task QueryHours_pullsCorrectHoursAndInfo_forUser()
        {
            var dateBefore = new DateTime(2018, 11, 30);
            var dateAfter = new DateTime(2019, 1, 15);
            await entryService.CreateBillableTimeEntry(dateBefore, 7, 1, 1);
            await entryService.CreateBillableTimeEntry(dateAfter, 7, 1, 1);

            var hours = await userReportService.QueryAllHours();

            hours.Count.Should().Be(2);
            hours.Sum(x => x.Hours).Should().Be(14);
            hours.First().TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);
            hours.Select(x => x.ProjectOrName).Should().Contain("au");
        }

        [Fact]
        public async Task GetHoursSummary_summarizesHoursCorrectly()
        {
            const int testMonth = 3;

            var utcNowYear = DateTime.UtcNow.Year;
            var dateBefore = new DateTime(2018, 11, 30);
            var dateAfter = new DateTime(utcNowYear, testMonth, 2);
            await entryService.CreateBillableTimeEntry(dateBefore, 7, 1, 1);
            await entryService.CreateNonBillableTimeEntry(dateBefore.AddDays(-1), 5, null,
                TimeEntryTypeEnum.Vacation);

            await entryService.CreateBillableTimeEntry(dateAfter, 4, 1, 1);
            await entryService.CreateBillableTimeEntry(dateAfter.AddDays(1), 8, 1, 1);
            await entryService.CreateBillableTimeEntry(dateAfter.AddDays(2), 8, 1, 1);
            await entryService.CreateBillableTimeEntry(dateAfter.AddMonths(-1), 8, 1, 1);

            await entryService.CreateNonBillableTimeEntry(dateAfter.AddDays(15), 5, null,
                TimeEntryTypeEnum.Vacation);
            await entryService.CreateNonBillableTimeEntry(dateAfter.AddDays(16), 6, "dr visit",
                TimeEntryTypeEnum.Sick);
            await entryService.CreateNonBillableTimeEntry(dateAfter.AddDays(17), 7, "PDA",
                TimeEntryTypeEnum.NonBillable);

            var hours = await userReportService.GetHoursSummaryDefaultMonthAndYtd();

            hours.CurrentMonthDisplay.Should().Be($"March {utcNowYear}");
            hours.BillableHoursMonth.Should().Be(20d);
            hours.BillableHoursYtd.Should().Be(28d);

            hours.SickHoursMonth.Should().Be(6d);
            hours.VacationHoursMonth.Should().Be(5d);
            hours.NonBillableHoursMonth.Should().Be(7d);

            hours.SickHoursYtd.Should().Be(6d);
            hours.VacationHoursYtd.Should().Be(5d);
            hours.NonBillableHoursYtd.Should().Be(7d);
        }
    }
}