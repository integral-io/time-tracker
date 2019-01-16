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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using TimeTracker.Api.Models;
using TimeTracker.Data;

namespace TimeTracker.Api.Test
{
    /// <summary>
    /// tests that bring up a webhost and startup application.
    /// </summary>
    public class SlackSlashCommandControllerTest : IClassFixture<CustomWAF>
    {
        private readonly HttpClient _client;
        
        public SlackSlashCommandControllerTest(CustomWAF fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task HandleCommand_hours_processesRecordOption()
        {
            string textCommand = "record Au 8 wfh";
            var response = await _client.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("team_id", "xxx"),
                new KeyValuePair<string, string>("user_id", "UT33423"),
                new KeyValuePair<string, string>("user_name", "James"),
                new KeyValuePair<string, string>("text",textCommand) // this part could become theory input 
            }));
            // project au does not exist and needs to be created before this will work. 
            // ideally we could get access to dbContext here so that we can then call the db. 
            string responseContent = await response.Content.ReadAsStringAsync();
            response.IsSuccessStatusCode.Should().BeTrue();
            SlackMessage message = JsonConvert.DeserializeObject<SlackMessage>(responseContent);
            message.Text.Should().Be("Registered *8.0 hours* for project *au* today. _Worked From Home_");
        }
    }
    
    /// <summary>
    /// to be able to change services injected, ie. data tier / or external http calls. 
    /// </summary>
    public class CustomWAF : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
           // builder.ConfigureServices()
            builder.ConfigureServices(x =>
            {
                // Todo: check to see if we actually use a different db, or need code to not re-instantiate real db
                x.AddDbContext<TimeTrackerDbContext>(options => { options.UseInMemoryDatabase("slack_hours"); });
            });
            base.ConfigureWebHost(builder);
        }
    }
}