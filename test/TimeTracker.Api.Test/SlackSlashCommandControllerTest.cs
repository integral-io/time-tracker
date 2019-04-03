using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Newtonsoft.Json;
using TimeTracker.Library.Models;

namespace TimeTracker.Api.Test
{
    public class SlackSlashCommandControllerTest : IClassFixture<HttpClientWithInMemoryDatabase>
    {
        private readonly HttpClient client;
        
        public SlackSlashCommandControllerTest(HttpClientWithInMemoryDatabase factory)
        {
            client = factory.Client;
        }

        [Fact]
        public async Task HandleCommand_hours_returnsHelpIfNoOptionPassed()
        {
            var response = await client.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("team_id", "xxx"),
                new KeyValuePair<string, string>("user_id", "UT33423"),
                new KeyValuePair<string, string>("user_name", "James"),
                new KeyValuePair<string, string>("text", null)
            }));

            response.IsSuccessStatusCode.Should().BeTrue();
            var responseContent = await response.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<SlackMessage>(responseContent);
            message.Text.Should().StartWith("*/hours* record <projectName>");
        }
    }
}