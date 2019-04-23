using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class HelpTest : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>
    {
        private readonly SlackMessageOrchestrator orchestrator;

        public HelpTest(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            var database = inMemoryDatabase.Database;
            orchestrator = new SlackMessageOrchestrator(database);
        }

        [Fact]
        public async Task WhenRequestingHelp_ShowsMessageWithAllCommands()
        {
            var payload = CreateHelpRequest("help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Contain("*/hours* record - Allows the user to record hours.").And
                .Contain("*/hours* report - Shows the users reported hours.").And
                .Contain("*/hours* delete - Deletes hours reported.").And
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
        public async Task WhenRequestingHelpForReport_ShowsSlackSpecificHelpMessage()
        {
            var payload = CreateHelpRequest("report help");

            var slackMessage = await orchestrator.HandleCommand(payload);

            slackMessage.Text.Should()
                .Contain("*/hours* report _generate default report of hours for week, month, and ytd_").And
                .Contain("*/hours* report month <month> <optional: year> _generate report of hours for month (ie. apr) default is current year_").And
                .Contain("*/hours* report year <year> _generate report of hours for year_").And
                .Contain("*/hours* report date <date> _generate report for day (include dashes)_");
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