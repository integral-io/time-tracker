using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class SlackMessageOrchestratorTest
    {
        [Fact]
        public async Task HandleCommand_deleteHours_returnsDeletedMessage_andDeletesHours()
        {
            using (var dc =
                new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions(Guid.NewGuid().ToString())))
            {
                // setup db any?
                // any other mocks?
                var orchestrator = new SlackMessageOrchestrator(dc);
                TestHelpers.AddClientAndProject(dc);
                var users = TestHelpers.AddTestUsers(dc);
                
                var sut = new TimeEntryService(users.First().UserId, dc);
                var date = DateTime.UtcNow.Date;
                await sut.CreateBillableTimeEntry(date, 7, 1, 1);

                var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
                {
                    text = "delete",
                    user_id = users.First().SlackUserId,
                    user_name = users.First().UserName
                });
                
                // assertions
                slackMessage.Text.Should().Be($"Deleted {7d:F1} hours for date: {date:D}");
                dc.TimeEntries.Count().Should().Be(0);
            }
        }
        
        [Fact]
        public async Task HandleCommand_hours_processesRecordOption()
        {
            using (var dc =
                new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions(Guid.NewGuid().ToString())))
            {
                var orchestrator = new SlackMessageOrchestrator(dc);
                TestHelpers.AddClientAndProject(dc);
                
                var utcNow = DateTime.UtcNow;
                var todayString = utcNow.ToString("D");

                var textCommand = "record Au 8 wfh";
                
                var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
                {
                    text = textCommand,
                    user_id = "UT33423",
                    user_name = "James"
                });
                
                slackMessage.Text.Should().Be($"Registered *8.0 hours* for project *au* {todayString}. _Worked From Home_");

                var timeEntry = await dc.TimeEntries.FirstOrDefaultAsync();
                timeEntry.Should().NotBeNull();
                timeEntry.Hours.Should().Be(8);
            }
        }

        [Fact]
        public async Task HandleCommand_hours_processRecordOption_shouldFailIfInvalidProjectName()
        {
            using (var dc =
                new TimeTrackerDbContext(TestHelpers.BuildInMemoryDatabaseOptions(Guid.NewGuid().ToString())))
            {
                var orchestrator = new SlackMessageOrchestrator(dc);
                TestHelpers.AddClientAndProject(dc);
                
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
        }
    }
}