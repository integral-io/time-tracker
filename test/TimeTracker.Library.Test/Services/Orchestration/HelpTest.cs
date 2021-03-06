using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;
using TimeTracker.TestInfra;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class HelpTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>
    {
        private readonly SlackMessageOrchestrator orchestrator;
        private const string WebAppUri = "https://localhost";

        public HelpTest(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            var database = inMemoryDatabase.Database;
            orchestrator = new SlackMessageOrchestrator(database, WebAppUri);
        }

        [Fact]
        public async Task WhenRequestingHelp_ShowsMessageWithAllCommands()
        {
            var payload = CreateHelpRequest("help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Contain("*/hours* record - Allows the user to record hours.").And
                .Contain("*/hours* summary - Shows the users reported hours.").And
                .Contain("*/hours* delete - Deletes hours reported.").And
                .Contain("*/hours* projects - Lists available projects (project - client).").And
                .Contain("*/hours* web - Links user to web for hours report.").And
                .Contain("Add 'help' to each one of the options to get specific help. ex: */hours* record help");
        }

        [Fact]
        public async Task WhenRequestingHelpForDelete_ShowsSlackSpecificHelpMessage()
        {
            var payload = CreateHelpRequest("delete help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Contain("*/hours* delete <optional: date> _delete all hours for the date_").And
                .Contain("*/hours* delete nonbill <optional: date> _delete nonbillable all hours for the date_").And
                .Contain("*/hours* delete sick <optional: date> _delete all sick hours for the date_").And
                .Contain("*/hours* delete vacation <optional: date> _delete all vacation hours for the date_").And
                .Contain("*/hours* delete billable <optional: date> _delete all billable hours for the date_");
        }

        [Fact]
        public async Task WhenRequestingHelpForRecord_ShowsSlackSpecificHelpMessage()
        {
            var payload = CreateHelpRequest("record help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Contain("*/hours* record <projectName> <hours> _Will use today date by default_").And
                .Contain("*/hours* record <projectName> <hours> jan-21 _sets date to january 21 current year_").And
                .Contain("*/hours* record <projectName> <hours> wfh _wfh option marks as worked from home_").And
                .Contain("*/hours* record nonbill <hours> <optional: date> \"non billable reason\" _non billable hour for a given reason, ie PDA_").And
                .Contain("*/hours* record sick <hours> <optional: date> _marks sick hours_").And
                .Contain("*/hours* record vacation <hours> <optional: date> _marks vacation hours_");
        }

        [Fact]
        public async Task WhenRequestingHelpForSummary_ShowsSlackSpecificHelpMessage()
        {
            var payload = CreateHelpRequest("summary help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Contain("*/hours* summary _generate default summary of hours for week, month, and ytd_").And
                .Contain("*/hours* summary month <month> <optional: year> _generate summary of hours for month (ie. apr) default is current year_").And
                .Contain("*/hours* summary year <year> _generate summary of hours for year_").And
                .Contain("*/hours* summary date <date> _generate summary for day (include dashes)_").And
                .Contain("*/hours* summary last _generate summary for last ten days_");
        } 

        [Fact]
        public async Task WhenRequestingHelpForProjects_ShowsSlackSpecificHelpMessage()
        {
            var payload = CreateHelpRequest("projects help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Be("*/hours* projects _display a list of available projects (project - client)_");
        }

        [Fact]
        public async Task WhenRequestingForWebHelp_ShowsSlackSpecificHelpMessage()
        {
            var payload = CreateHelpRequest("web help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Be("*/hours* web - _generate a link to a report of hours_");
        }
        
        private static SlashCommandPayload CreateHelpRequest(string helpText)
        {
            return new SlashCommandPayload
            {
                text = helpText
            };
        }
    }
}