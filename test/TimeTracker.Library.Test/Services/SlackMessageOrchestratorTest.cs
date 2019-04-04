using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class SlackMessageOrchestratorTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;

        public SlackMessageOrchestratorTest(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            database = inMemoryDatabase.Database;

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
        public async Task WhenTimeEntryStoredOnFirstOfMonth_ThenItGetsDisplayedInReport()
        {
            var user = database.Users.First();
            
            var now = DateTime.UtcNow;
            var timeEntry = new TimeEntry
            {
                Hours = 8, 
                Date = new DateTime(now.Year, now.Month, 1),
                TimeEntryType = TimeEntryTypeEnum.NonBillable,
                User = user
            };
            
            database.TimeEntries.Add(timeEntry);
            await database.SaveChangesAsync();
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "report",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });

            response.Text.Should().Contain("Non-billable Hours: 8.0");
        }
    }
}