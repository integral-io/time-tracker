using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class ProjectsTests : IClassFixture<InMemoryDatabaseWithProjectsAndUsers>
    {
        private readonly SlackMessageOrchestrator orchestrator;
        private readonly TimeTrackerDbContext database;

        public ProjectsTests(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            database = inMemoryDatabase.Database;
            orchestrator = new SlackMessageOrchestrator(database);
        }
        
        [Fact]
        public async Task WhenRequestingProjects_ShowsAllProjects()
        {
            var auClient = new BillingClient
            {
                BillingClientId = 2,
                Name = "Au",
                AddressLine1 = "123 Street",
                AddressLine2 = "",
                CityStateZip = "DetroitMI48226",
                Email = "au@au.com"
            };

            database.BillingClients.Add(auClient);
            
            var vsmProject = new Project
            { 
                ProjectId = 5,
                Name = "VSM",
                BillingClientId = 2,
                Start = DateTime.Now,
                End  = DateTime.UtcNow,
                BillingClient = auClient
 
            };
            database.Projects.Add(vsmProject);
            database.SaveChanges();
        
            
            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "projects",
                user_id = "UT33423",
                user_name = "James"
            });

            slackMessage.Text.Should()
                .Contain("VSM").And
                .Contain("au");

        }


    }
}