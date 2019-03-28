using System.Collections.Immutable;
using TimeTracker.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models.Admin;

namespace TimeTracker.Library.Services
{
    /// <summary>
    /// Reporting service for admin users to use that provide all user data
    /// </summary>
    public class AdminReportService
    {
        private readonly TimeTrackerDbContext db;

        public AdminReportService(in TimeTrackerDbContext db)
        {
            this.db = db;
        }

        public async Task<IImmutableList<UserReport>> GetAllUsersReport()
        {
            var query = from u in db.Users
                select new UserReport
                {
                    SlackUserName = u.UserName,
                    Last = u.LastName,
                    First = u.FirstName,
                    BillableHoursYtd = u.TimeEntries.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.BillableProject).Sum(x=>x.Hours),
                    OtherNonBillableYtd = u.TimeEntries.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable).Sum(x=>x.Hours),
                    SickHoursYtd = u.TimeEntries.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick).Sum(x=>x.Hours),
                    VacationHoursYtd = u.TimeEntries.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation).Sum(x=>x.Hours)
                };
            return (await query.ToListAsync()).ToImmutableList();
        }
    }
}