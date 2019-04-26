using System;
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

        [Fact]
        public async Task SaveGoogleInfo_savesExpectedData()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("users-2");
            string slackUsername = "harry-slack";
            string slackUserId = "userId";
            string googleId = Guid.NewGuid().ToString();
            string first = "first";
            string last = "last";
            string email = "email@bobby.com";

            using (var context = new TimeTrackerDbContext(options))
            {
                var sut = new UserService(context);
                var userCreated = await sut.FindOrCreateSlackUser(slackUserId, slackUsername);

                await sut.SaveGoogleInfo(slackUserId, googleId, first, last, email);
            }

            using (var context = new TimeTrackerDbContext(options))
            {
                var dbUser = context.Users.FirstOrDefault(x => x.SlackUserId == slackUserId);
                dbUser.LastName.Should().Be(last);
                dbUser.FirstName.Should().Be(first);
                dbUser.GoogleIdentifier.Should().Be(googleId);
                dbUser.OrganizationEmail.Should().Be(email);
                
            }
        }

        [Fact]
        public async Task GetUserIdFromGoogleId_getsCorrectGuid()
        {
            var options = TestHelpers.BuildInMemoryDatabaseOptions("users-3");
            string googleId = Guid.NewGuid().ToString();
            string first = "first";
            string last = "last";
            string email = "email@bobby.com";

            string slackUserId = "id";
            using (var context = new TimeTrackerDbContext(options))
            {
                var userService = new UserService(context);
                var user = userService.FindOrCreateSlackUser(slackUserId, "username");
                await userService.SaveGoogleInfo(slackUserId, googleId, first, last, email);
                var userId = await userService.GetUserIdFromGoogleId(googleId);
                userId.Should().Be(user.Result.UserId);
            }
        }
    }
}