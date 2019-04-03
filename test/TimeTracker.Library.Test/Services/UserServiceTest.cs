using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Library.Services;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using Xunit;

namespace TimeTracker.Library.Test.Services
{
    public class UserServiceTest
    {

        [Fact]
        public async Task FindOrCreateSlackUser_savesNewUserAndReturnsExisting()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("users");

            User userCreated;
            var slackUsername = "userName";
            
            using (var context = new TimeTrackerDbContext(options))
            {
                var sut = new UserService(context);
                userCreated = await sut.FindOrCreateSlackUser("userId", slackUsername);

                var userFound = await sut.FindOrCreateSlackUser("userId", "whatever");

                userFound.UserId.Should().Be(userCreated.UserId);
            }

            using (var context = new TimeTrackerDbContext(options))
            {
                var firstUserDb = context.Users.FirstOrDefault(x => x.UserName == slackUsername);
                userCreated.UserId.Should().Be(firstUserDb.UserId);
            }
        }
    }
}