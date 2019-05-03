using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TimeTracker.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
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
            var users = from u in db.Users select u;
            if (projectId.HasValue)
            {
                users = users.Where(x => x.TimeEntries.Any(t => t.Date >= start && t.Date <= end && t.ProjectId == projectId));
            }

            var timeEntries = from t in db.TimeEntries
                where t.Date >= start && t.Date <= end
                select t;
            if (projectId.HasValue)
            {
                timeEntries = timeEntries.Where(x => x.ProjectId == projectId);
            }

            var timeEntriesList = await timeEntries.ToListAsync();
            
            var projection = from u in await users.ToListAsync()
                select new UserReport
                {
                    SlackUserName = u.UserName,
                    Last = u.LastName,
                    First = u.FirstName,
                    BillableHoursYtd = timeEntriesList
                        .Where(x=> x.UserId == u.UserId && x.TimeEntryType == TimeEntryTypeEnum.BillableProject)
                        .Sum(x=>x.Hours),
                    OtherNonBillableYtd = timeEntriesList
                        .Where(x=> x.UserId == u.UserId && x.TimeEntryType == TimeEntryTypeEnum.NonBillable)
                        .Sum(x=>x.Hours),
                    SickHoursYtd = timeEntriesList
                        .Where(x=> x.UserId == u.UserId && x.TimeEntryType == TimeEntryTypeEnum.Sick)
                        .Sum(x=>x.Hours),
                    VacationHoursYtd = timeEntriesList
                        .Where(x=> x.UserId == u.UserId && x.TimeEntryType == TimeEntryTypeEnum.Vacation)
                        .Sum(x=>x.Hours)
                };
            return (projection).ToImmutableList();
        }
    }
}