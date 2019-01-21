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

namespace TimeTracker.Api.Test
{
    /// <summary>
    /// tests that bring up a webhost and startup application.
    /// </summary>
    public class SlackSlashCommandControllerTest : IClassFixture<CustomWAF>
    {
        private readonly HttpClient _client;
        private readonly DbContextOptions<TimeTrackerDbContext> _options;
        
        public SlackSlashCommandControllerTest(CustomWAF fixture)
        {
            _options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
                .UseInMemoryDatabase(fixture.InMemoryDbName)
                .Options;
            _client = fixture.CreateClient();
            
        }

        [Fact]
        public async Task HandleCommand_hours_processesRecordOption()
        {
            AddClientAndProject();
            DateTime utcNow = DateTime.UtcNow;
            string todayString = utcNow.ToString("D");
            
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
            message.Text.Should().Be($"Registered *8.0 hours* for project *au* {todayString}. _Worked From Home_");
        }

        [Fact]
        public async Task HandleCommand_hours_processRecordOption_shouldFailIfInvalidProjectName()
        {
            AddClientAndProject();

            var recordInvalidProjectName = "INVALID-PROJECT-NAME".ToLower();
            string textCommand = $"record {recordInvalidProjectName} 8";
            
            var response = await _client.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("team_id", "xxx"),
                new KeyValuePair<string, string>("user_id", "UT33423"),
                new KeyValuePair<string, string>("user_name", "James"),
                new KeyValuePair<string, string>("text",textCommand) // this part could become theory input 
            }));
            string responseContent = await response.Content.ReadAsStringAsync();
            response.IsSuccessStatusCode.Should().BeTrue();
            SlackMessage message = JsonConvert.DeserializeObject<SlackMessage>(responseContent);
            message.Text.Should().Be($"Error: *Invalid Project Name {recordInvalidProjectName}*");
        }

        private void AddClientAndProject()
        {
            var context = new TimeTrackerDbContext(_options);
            
                context.BillingClients.Add(new BillingClient()
                {
                    BillingClientId = 1,
                    Name = "Autonomic"
                });
                context.Projects.Add(new Project()
                {
                    ProjectId = 1,
                    BillingClientId = 1,
                    Name = "au"
                });
                context.SaveChanges();
            
        }
    }
    
    /// <summary>
    /// to be able to change services injected, ie. data tier / or external http calls. 
    /// </summary>
    public class CustomWAF : WebApplicationFactory<Startup>
    {
        public readonly string InMemoryDbName = "slack-controller-hours";
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(x =>
            {
                x.AddDbContext<TimeTrackerDbContext>(options => { options.UseInMemoryDatabase(InMemoryDbName); });
            });
            

            base.ConfigureWebHost(builder);
        }
    }
}