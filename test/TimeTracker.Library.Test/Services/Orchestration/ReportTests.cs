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