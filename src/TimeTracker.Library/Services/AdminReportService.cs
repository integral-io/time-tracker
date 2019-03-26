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
        private const int _payPeriodLength = 14;

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
        public async Task<IImmutableList<UserReport>> GetAllUsersByDate(DateTime start, DateTime? end = null)
        {
            if (!end.HasValue)
            {
                //If not provided, use default pay period length, 14 days.
                end = start.AddDays(_payPeriodLength);
            }
            var query = from u in db.Users
                select new UserReport
                {
                    SlackUserName = u.UserName,
                    Last = u.LastName,
                    First = u.FirstName,
                    BillableHoursYtd = u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.BillableProject && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours),
                    OtherNonBillableYtd = u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours),
                    SickHoursYtd = u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours),
                    VacationHoursYtd = u.TimeEntries
                        .Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation && x.Date >= start && x.Date <= end)
                        .Sum(x=>x.Hours)
                };
            return (await query.ToListAsync()).ToImmutableList();
        }
    }
}