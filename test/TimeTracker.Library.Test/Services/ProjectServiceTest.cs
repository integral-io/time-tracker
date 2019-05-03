using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Library.Services;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.TestInfra;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class ProjectServiceTest
    {
        [Fact]
        public async Task FindProjectFromName_returnsExistingProject()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("projects");

            var projectName = "bobby";
            
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
                var sut = new ProjectService(context);
                var project = await sut.FindProjectFromName(projectName);

                project.Should().NotBeNull();
                project.Name.Should().Be(projectName);
            }
        }
    }
}