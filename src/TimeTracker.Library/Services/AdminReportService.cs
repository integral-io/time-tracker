using System;
using System.Collections.Generic;
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

        /// <summary>
        /// This function returns an "Hours Report" filtered by date.
        /// </summary>
        /// <param name="start">Start date for query, inclusive.</param>
        /// <param name="end">End date for query, inclusive.</param>
        /// <returns></returns>
        public async Task<IImmutableList<UserReport>> GetAllUsersByDate(DateTime start, DateTime end, int? projectId = null)
        {
            var query = from u in db.Users select u;
            if (projectId.HasValue)
            {
                query = query.Where(x => x.TimeEntries.Any(t => t.ProjectId == projectId));
            }
            
            var projection = from u in query
                select new UserReport
                {
                    SlackUserName = u.UserName,
                    Last = u.LastName,
                    First = u.FirstName,
                    BillableHoursYtd = u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.BillableProject && x.Date >= start && x.Date <= end
                                  && (projectId == null || projectId.HasValue && x.ProjectId == projectId))
                        .Sum(x=>x.Hours),
                    OtherNonBillableYtd = projectId.HasValue ? 0 : 
                        u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours),
                    SickHoursYtd = projectId.HasValue ? 0 :
                        u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours),
                    VacationHoursYtd = projectId.HasValue ? 0 :
                        u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours)
                };
            return (await projection.ToListAsync()).ToImmutableList();
        }
    }
}