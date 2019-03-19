using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
                SlackMessageOrchestrator orchestrator = new SlackMessageOrchestrator(dc);
                TestHelpers.AddClientAndProject(dc);
                var users = TestHelpers.AddTestUsers(dc);
                
                TimeEntryService sut = new TimeEntryService(users.First().UserId, dc);
                DateTime date = DateTime.UtcNow.Date;
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
    }
}