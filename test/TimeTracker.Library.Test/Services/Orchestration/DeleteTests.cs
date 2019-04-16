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

        // todo improve testing of this command.
        [Fact]
        public async Task HandleCommand_deleteBillableHoursType_returnsDeletedMessage_andOnlyDeletesBillableHoursForThatType()
        {
            var user = database.Users.First();
            var date = DateTime.UtcNow.Date;
            var timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date, 5, "dr visit", TimeEntryTypeEnum.Sick);
            await timeEntryService.CreateBillableTimeEntry(date.AddDays(-1), 3, 1, 1);
//            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-1), 4, "Maui", TimeEntryTypeEnum.Vacation);
//            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(-1), 2, "beach", TimeEntryTypeEnum.NonBillable);

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "delete billable",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            slackMessage.Text.Should().Be($"Deleted {4d:F1} {TimeEntryTypeEnum.BillableProject} hours for date: {date:D}");
            database.TimeEntries.Count().Should().Be(2);
            database.TimeEntries.Where(x => x.UserId == user.UserId).Sum(x => x.Hours).Should().Be(8);
            database.TimeEntries.Where(x => x.UserId == user.UserId && x.TimeEntryType == TimeEntryTypeEnum.BillableProject && x.Date == date).ToList().Count.Should().Be(0);

//            database.Users.First(x => x.UserId == user.UserId).TimeEntries.Where(x => x.TimeEntryType == TimeEntryTypeEnum.BillableProject && x.Date == date).ToList().Count.Should().Be(0);
//            database.Users.First(x => x.UserId == user.UserId).TimeEntries.Sum(x => x.Hours).Should().Be(8);
        }
    }
}