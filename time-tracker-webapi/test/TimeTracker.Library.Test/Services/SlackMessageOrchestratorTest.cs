using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class SlackMessageOrchestratorTest : IClassFixture<InMemoryDatabase>
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;

        public SlackMessageOrchestratorTest(InMemoryDatabase inMemoryDatabase)
        {
            database = inMemoryDatabase.Database;

            orchestrator = new SlackMessageOrchestrator(database);
        }

        [Fact]
        public async Task HandleCommand_deleteHours_returnsDeletedMessage_andDeletesHours()
        {
            var users = TestHelpers.AddTestUsers(database);
            var sut = new TimeEntryService(users.First().UserId, database);
            var date = DateTime.UtcNow.Date;
            await sut.CreateBillableTimeEntry(date, 7, 1, 1);

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "delete",
                user_id = users.First().SlackUserId,
                user_name = users.First().UserName
            });

            slackMessage.Text.Should().Be($"Deleted {7d:F1} hours for date: {date:D}");
            database.TimeEntries.Count().Should().Be(0);
        }

        [Fact]
        public async Task HandleCommand_hours_processesRecordOption()
        {
            var utcNow = DateTime.UtcNow;
            var todayString = utcNow.ToString("D");
            var textCommand = "record Au 8 wfh";

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = textCommand,
                user_id = "UT33423",
                user_name = "James"
            });

            slackMessage.Text.Should()
                .Be($"Registered *8.0 hours* for project *au* {todayString}. _Worked From Home_");

            var timeEntry = await database.TimeEntries.FirstOrDefaultAsync();
            timeEntry.Should().NotBeNull();
            timeEntry.Hours.Should().Be(8);
        }

        [Fact]
        public async Task HandleCommand_hours_processRecordOption_shouldFailIfInvalidProjectName()
        {
            var recordInvalidProjectName = "INVALID-PROJECT-NAME".ToLower();
            var textCommand = $"record {recordInvalidProjectName} 8";

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = textCommand,
                user_id = "UT33423",
                user_name = "James"
            });

            slackMessage.Text.Should().Be($"Error: *Invalid Project Name {recordInvalidProjectName}*");
        }

        [Fact]
        public async Task HandleCommand_hours_processRecordOption_shouldFailIfNotEntirelyInterpreted()
        {
            const string textCommand = "record au 8 some nonsense";

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = textCommand,
                user_id = "UT33423",
                user_name = "James"
            });

            database.TimeEntries.Should().BeEmpty();
            slackMessage.Text.Should().Be($"Error: *Not sure how to interpret 'some nonsense'*");
        }
    }
}