using System.Collections.Generic;
using System.Collections.Immutable;
using TimeTracker.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Library.Models.Admin;

namespace TimeTracker.Library.Services
{
    /// <summary>
    /// Reporting service for admin users to use that provide all user data
    /// </summary>
    public class AdminReportService
    {
        private readonly TimeTrackerDbContext _db;

        public AdminReportService(in TimeTrackerDbContext db)
        {
            _db = db;
        }

        public async Task<IImmutableList<UserReport>> GetAllUsersReport()
        {
            var query = from u in _db.Users
                select new UserReport
                {
                    SlackUserName = u.UserName,
                    Last = u.LastName,
                    First = u.FirstName,
                    BillableHoursYtd = 0d
                };
            return (await query.ToListAsync()).ToImmutableList();
        }
    }
}