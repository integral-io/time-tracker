using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions.Common;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using TimeTracker.Api.Models;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Api.Test
{
    /// <summary>
    /// tests that bring up a webhost and startup application.
    /// </summary>
    public class SlackSlashCommandControllerTest : IClassFixture<CustomWAFAppTester<TimeTracker.Api.Startup>>
    {
        private readonly CustomWAFAppTester<TimeTracker.Api.Startup> _factory;
        private readonly HttpClient _client;
        
        public SlackSlashCommandControllerTest(CustomWAFAppTester<TimeTracker.Api.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                // config options
            });
        }

        [Fact]
        public async Task HandleCommand_hours_returnsHelpIfNoOptionPassed()
        {
            string textCommand = null;
            var response = await _client.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("team_id", "xxx"),
                new KeyValuePair<string, string>("user_id", "UT33423"),
                new KeyValuePair<string, string>("user_name", "James"),
                new KeyValuePair<string, string>("text", textCommand)
            }));
            
            string responseContent = await response.Content.ReadAsStringAsync();
            response.IsSuccessStatusCode.Should().BeTrue();
            SlackMessage message = JsonConvert.DeserializeObject<SlackMessage>(responseContent);
            message.Text.Should().StartWith("*/hours* record <projectName>");
        }
    }
}