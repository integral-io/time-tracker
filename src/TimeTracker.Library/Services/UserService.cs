using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library.Services
{
    public class UserService
    {
        private readonly TimeTrackerDbContext db;
        
        public UserService(TimeTrackerDbContext db)
        {
            this.db = db;
        }

        public async Task<User> FindOrCreateSlackUser(string slackUserId, string slackUsername)
        {
            Guard.ThrowIfCheckFails(!string.IsNullOrEmpty(slackUserId), "cannot be null or empty", nameof(slackUserId));
            Guard.ThrowIfCheckFails(!string.IsNullOrEmpty(slackUsername), "cannot be null or empty", nameof(slackUsername));
            
            var user = await db.Users.FirstOrDefaultAsync(
                x => x.SlackUserId.Equals(slackUserId, StringComparison.InvariantCultureIgnoreCase));

            if (user == null)
            {
                user = new User()
                {
                    SlackUserId = slackUserId,
                    UserId = Guid.NewGuid(),
                    UserName = slackUsername
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            return user;
        }

        public async Task SaveGoogleInfo(string slackUserId, string googleIdentifier, string first, string last, string email)
        {
            Guard.ThrowIfCheckFails(!string.IsNullOrEmpty(slackUserId), "cannot be null or empty", nameof(slackUserId));
            Guard.ThrowIfCheckFails(!string.IsNullOrEmpty(googleIdentifier), "cannot be null or empty", nameof(googleIdentifier));

            var user = await db.Users.FirstOrDefaultAsync(
                x => x.SlackUserId.Equals(slackUserId, StringComparison.InvariantCultureIgnoreCase));

            Guard.ThrowIfCheckFails(user != null, "must have existing slack user", nameof(user));

            user.FirstName = first;
            user.LastName = last;
            user.GoogleIdentifier = googleIdentifier;
            user.OrganizationEmail = email;

            db.Users.Update(user);
        }
    }
}