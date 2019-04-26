using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class RecordTests
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;
        private const string WebAppUri = "https://localhost";

        public RecordTests()
        {
            database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            orchestrator = new SlackMessageOrchestrator(database, WebAppUri);
        }

        [Fact]
        public async Task HandleCommand_hours_processesRecordOption()
        {
            var todayString = DateTime.UtcNow.ToString("D");
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
        
        [Fact]
        public async Task WhenRecordSickHours_SlackMessageIncludesSickHoursWereRecorded()
        {
            var todayString = DateTime.UtcNow.ToString("D");
            var textCommand = "record sick 3 flu";

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = textCommand,
                user_id = "UT33423",
                user_name = "James"
            });

            slackMessage.Text.Should()
                .Be($"Registered *3.0 hours* for Sick reason: flu for date: {todayString}");

            var timeEntry = await database.TimeEntries.FirstOrDefaultAsync();
            timeEntry.Should().NotBeNull();
            timeEntry.Hours.Should().Be(3);
        }
        
        [Fact]
        public async Task WhenRecordVacationHours_SlackMessageIncludesVacationHoursWereRecorded()
        {
            var todayString = DateTime.UtcNow.ToString("D");
            var textCommand = "record vacation 5";

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = textCommand,
                user_id = "UT33423",
                user_name = "James"
            });

            slackMessage.Text.Should()
                .Be($"Registered *5.0 hours* for Vacation for date: {todayString}");

            var timeEntry = await database.TimeEntries.FirstOrDefaultAsync();
            timeEntry.Should().NotBeNull();
            timeEntry.Hours.Should().Be(5);
        }



    }
}