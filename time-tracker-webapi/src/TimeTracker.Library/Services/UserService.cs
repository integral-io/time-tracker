using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Library.Services
{
    public class UserService
    {
        private readonly TimeTrackerDbContext _db;
        
        public UserService(in TimeTrackerDbContext db)
        {
            _db = db;
        }

        public async Task<User> FindOrCreateSlackUser(string slackUserId, string slackUsername)
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                x => x.SlackUserId.Equals(slackUserId, StringComparison.InvariantCultureIgnoreCase));

            if (user == null)
            {
                user = new User()
                {
                    SlackUserId = slackUserId,
                    UserId = Guid.NewGuid(),
                    UserName = slackUsername
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            return user;
        }
    }
}