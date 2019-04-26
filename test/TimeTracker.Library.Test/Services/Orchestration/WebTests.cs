using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class WebTests
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;
        private const string WebAppUri = "https://localhost";

        public WebTests()
        {
            database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            orchestrator = new SlackMessageOrchestrator(database, WebAppUri);
        }

        [Fact]
        public async Task HandleCommand_web_returnsWebLinkInstructionMessage_andProvidesWebLink()
        {
            var user = database.Users.First();
            var timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(DateTime.UtcNow.Date, 7, 1, 1);

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "web",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });

            var link = $"{WebAppUri}/account/linkslack?slackuser=" + user.SlackUserId;

            slackMessage.Text.Should().Be($"Click this link {link} to access your hours on the web.");
        }
    }
}