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
    public class DeleteTests
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;

        public DeleteTests()
        {
            database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            orchestrator = new SlackMessageOrchestrator(database);
        }

        [Fact]
        public async Task HandleCommand_deleteHours_returnsDeletedMessage_andDeletesHours()
        {
            var user = database.Users.First();
            var timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow.Date, 7, 1, 1);

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "delete",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });

            slackMessage.Text.Should().Be($"Deleted {7d:F1} hours for date: {DateTime.UtcNow.Date:D}");
            database.TimeEntries.Count().Should().Be(0);
        }

        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable, "nonbill", 4, 1)]
        [InlineData(TimeEntryTypeEnum.NonBillable, "nonbillable", 4, 1)]
        [InlineData(TimeEntryTypeEnum.Sick, "sick", 3, 1)]
        [InlineData(TimeEntryTypeEnum.Vacation, "vacation", 2, 1)]
        [InlineData(TimeEntryTypeEnum.BillableProject, "billable", 3, 2)]
        public async Task
            HandleCommand_deleteHoursTypeDefaultDay_returnsDeletedMessage_andOnlyDeletesHoursForThatTypeToday(
                TimeEntryTypeEnum entryType, string reportText, int hours, int numEntriesToDelete)
        {
            var user = database.Users.First();
            var date = DateTime.UtcNow.Date;
            var timeEntryService = new TimeEntryService(user.UserId, database);
           
            await SetUpHourEntriesAndDays(timeEntryService, date);
            var numEntries = database.TimeEntries.Count();
            
            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "delete " + reportText,
                user_id = user.SlackUserId,
                user_name = user.UserName
            });

            slackMessage.Text.Should().Be($"Deleted {hours:F1} {entryType} hours for date: {date:D}");
            database.TimeEntries.Count().Should().Be(numEntries - numEntriesToDelete);
            database.TimeEntries.Where(x => x.UserId == user.UserId).Sum(x => x.Hours).Should().Be(33 - hours);
            database.TimeEntries.Where(x => x.UserId == user.UserId && x.TimeEntryType == entryType && x.Date == date)
                .ToList().Count.Should().Be(0);
        }

        private static async Task SetUpHourEntriesAndDays(TimeEntryService timeEntryService, DateTime date)
        {
            await timeEntryService.CreateNonBillableTimeEntry(date, 4, "beach", TimeEntryTypeEnum.NonBillable);
            await timeEntryService.CreateNonBillableTimeEntry(date, 3, "dr visit", TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateNonBillableTimeEntry(date, 2, "Maui", TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateBillableTimeEntry(date, 1, 1, 1);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);

            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-1), 6, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-2), 7, "Lansing",
                TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-3), 8, "time tracker",
                TimeEntryTypeEnum.NonBillable);
        }
    }
}