using System;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Api.Services;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using Xunit;

namespace TimeTracker.Api.Test.Services
{
    public class ProjectServiceTest
    {
        [Fact]
        public async Task FindProjectFromName_returnsExistingProject()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("projects");

            string projectName = "bobby";
            
            using (var context = new TimeTrackerDbContext(options))
            {
                context.Add(new Project()
                {
                    BillingClientId = 1,
                    Name = projectName
                });
                context.SaveChanges();
            }

            using (var context = new TimeTrackerDbContext(options))
            {
                var sut = new ProjectService(Guid.NewGuid(), context);
                var project = await sut.FindProjectFromName(projectName);

                project.Should().NotBeNull();
                project.Name.Should().Be(projectName);
            }
        }
    }
}