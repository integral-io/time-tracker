using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using TimeTracker.Api.Services;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using FluentAssertions.Common;
using Xunit;

namespace TimeTracker.Api.Test.Services
{
    public class UserServiceTest
    {

        [Fact]
        public async Task FindOrCreateSlackUser_savesNewUserAndReturnsExisting()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("users");

            User userCreated;
            string slackUsername = "userName";
            
            using (var context = new TimeTrackerDbContext(options))
            {
                var sut = new UserService(context);
                userCreated = await sut.FindOrCreateSlackUser("userId", slackUsername);

                User userFound = await sut.FindOrCreateSlackUser("userId", "whatever");

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