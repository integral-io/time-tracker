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
        private readonly CustomWAF _fixture;
        
        public SlackSlashCommandControllerTest(CustomWAF fixture)
        {
            _fixture = fixture;
        }

        private Tuple<HttpClient, TimeTrackerDbContext> CreateTestClient(string inMemoryDbName)
        {
            _fixture.InMemoryDbName = inMemoryDbName;
            
            var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
                .UseInMemoryDatabase(inMemoryDbName)
                .Options;
            
            var dbContext = new TimeTrackerDbContext(options);
            
            return new Tuple<HttpClient, TimeTrackerDbContext>(_fixture.CreateClient(), dbContext);
        }

        // Have not been able to get this test to pass - seems like in memory DB getting mixed up
        [Fact]
        public async Task HandleCommand_hours_processesRecordOption()
        {
            string testDbName = "hours-record";
            var tuple = CreateTestClient(testDbName);
            
            AddClientAndProject(tuple.Item2);
            DateTime utcNow = DateTime.UtcNow;
            string todayString = utcNow.ToString("D");

            string textCommand = "record Au 8 wfh";
            var response = await tuple.Item1.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("team_id", "xxx"),
                new KeyValuePair<string, string>("user_id", "UT33423"),
                new KeyValuePair<string, string>("user_name", "James"),
                new KeyValuePair<string, string>("text", textCommand) // this part could become theory input 
            }));
            
            string responseContent = await response.Content.ReadAsStringAsync();
            response.IsSuccessStatusCode.Should().BeTrue();
            
            var timeEntry = await tuple.Item2.TimeEntries.FirstOrDefaultAsync();
            timeEntry.Should().NotBeNull();
            timeEntry.Hours.Should().Be(8);
            
            SlackMessage message = JsonConvert.DeserializeObject<SlackMessage>(responseContent);
            message.Text.Should().Be($"Registered *8.0 hours* for project *au* {todayString}. _Worked From Home_");
        }

        [Fact]
        public async Task HandleCommand_hours_processRecordOption_shouldFailIfInvalidProjectName()
        {
            string testDbName = "hours-record-invalid-project";
            var tuple = CreateTestClient(testDbName);
            AddClientAndProject(tuple.Item2);

            var recordInvalidProjectName = "INVALID-PROJECT-NAME".ToLower();
            string textCommand = $"record {recordInvalidProjectName} 8";
            
            var response = await tuple.Item1.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new []
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

        private void AddClientAndProject(TimeTrackerDbContext dbContext)
        {
            dbContext.BillingClients.Add(new BillingClient()
            {
                BillingClientId = 1,
                Name = "Autonomic"
            });
            dbContext.Projects.Add(new Project()
            {
                ProjectId = 1,
                BillingClientId = 1,
                Name = "au"
            });
            dbContext.SaveChanges();
        }
    }
    
    /// <summary>
    /// to be able to change services injected, ie. data tier / or external http calls. 
    /// </summary>
    public class CustomWAF : WebApplicationFactory<Startup>
    {
        public string InMemoryDbName { get; set; }
        
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