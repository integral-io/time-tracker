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
        public async Task HandleCommand_hours_returnsSuccessMessage()
        {
            var response = await _client.PostAsync("/slack/slashcommand/hours", new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("team_id", "xxx"),
                new KeyValuePair<string, string>("user_id", "UT33423"),
                new KeyValuePair<string, string>("user_name", "James"),
                new KeyValuePair<string, string>("text","record au 8 wfh") // this part could become theory input 
            }));
            response.IsSuccessStatusCode.Should().BeTrue();
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