using System;
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
        private const string WebAppUri = "https://localhost";

        public ProjectsTests(InMemoryDatabaseWithProjectsAndUsers inMemoryDatabase)
        {
            database = inMemoryDatabase.Database;
            orchestrator = new SlackMessageOrchestrator(database, WebAppUri);
        }
        
        [Fact]
        public async Task WhenRequestingProjects_ShowsAllProjects()
        {
            createAuClientAndProject();
            createFakeProject();

            var slackMessage = await orchestrator.HandleCommand(new SlashCommandPayload()
            {
                text = "projects",
                user_id = "UT33423",
                user_name = "James"
            });

            slackMessage.Text.Should()
                .Contain("VSM").And
                .Contain("au").And
                .Contain("proyecto").And
                .Contain("no client");

        }

        private void createAuClientAndProject()
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
                End = DateTime.UtcNow,
                BillingClient = auClient
            };
            database.Projects.Add(vsmProject);
            database.SaveChanges();
        }
        
        private void createFakeProject()
        {
            var client = new BillingClient
            {
                BillingClientId = 3,
                Name = null,
                AddressLine1 = null,
                AddressLine2 = null,
                CityStateZip = null,
                Email = null
            };

            database.BillingClients.Add(client);

            var project = new Project
            {
                ProjectId = 6,
                Name = "proyecto",
                BillingClientId = 3,
                Start = null,
                End = DateTime.UtcNow,
                BillingClient = client
            };
            database.Projects.Add(project);
            database.SaveChanges();
        }
    }
}