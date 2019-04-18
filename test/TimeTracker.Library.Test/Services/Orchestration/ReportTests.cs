using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class ReportTests
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;

        public ReportTests()
        {
            database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            orchestrator = new SlackMessageOrchestrator(database);
        }

        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable, "Other Non-billable")]
        [InlineData(TimeEntryTypeEnum.Sick, "Sick")]
        [InlineData(TimeEntryTypeEnum.Vacation, "Vacation")]
        [InlineData(TimeEntryTypeEnum.BillableProject, "Billable")]
        public async Task WhenTimeEntryStoredOnFirstOfMonth_ThenItGetsDisplayedInReportCurrentMonth(TimeEntryTypeEnum entryType, string reportText)
        {
            var user = database.Users.First();
            var now = DateTime.UtcNow;
            await StoreTimeEntryOnFirstDayOfMonth(entryType, user, 8, now.Month);
            
            var response = await RequestReport(user);
            
            response.Text.Should().Contain($"{now:MMMM yyyy} {reportText} Hours: 8.0");
        }

        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable, "Other Non-billable")]
        [InlineData(TimeEntryTypeEnum.Sick, "Sick")]
        [InlineData(TimeEntryTypeEnum.Vacation, "Vacation")]
        [InlineData(TimeEntryTypeEnum.BillableProject, "Billable")]
        public async Task WhenTimeEntryStoredOnFirstDayOfYear_ThenItGetsDisplayedInReportYtd(TimeEntryTypeEnum entryType, string reportText)
        {
            var user = database.Users.First();
            await StoreTimeEntryOnFirstDayOfMonth(entryType, user, 8, 1);

            var response = await RequestReport(user);

            response.Text.Should().Contain($"YTD Total {reportText} Hours: 8.0");
        }

        [Fact]
        public async Task WhenRequestingReportForSpecificMonth_ReportOnlyIncludesHoursForTheMonth()
        {
            DateTime date = new DateTime(2019, 4, 18);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);            
            
            DateTime mayDate = new DateTime(2019, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report apr-2019",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"{date.Month} {date.Year} Total Billable Hours: 2.0");
            response.Text.Should().Contain($"{date.Month} {date.Year} Total Vacation Hours: 3.0");
            response.Text.Should().Contain($"{date.Month} {date.Year} Total Sick Hours: 1.0");
            response.Text.Should().Contain($"{date.Month} {date.Year} Total Other Non-Billable Hours: 0.0");
        }

        private Task<SlackMessage> RequestReport(User user)
        {
            return orchestrator.HandleCommand(new SlashCommandPayload
            {
                text = "report",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
        }

        private async Task StoreTimeEntryOnFirstDayOfMonth(TimeEntryTypeEnum entryType, User user, int hours, int month)
        {
            var timeEntry = new TimeEntry
            {
                Hours = hours,
                Date = new DateTime(DateTime.UtcNow.Year, month, 1),
                TimeEntryType = entryType,
                User = user
            };

            database.TimeEntries.Add(timeEntry);
            await database.SaveChangesAsync();
        }
    }
}