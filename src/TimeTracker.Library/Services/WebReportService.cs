using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models.Admin;
using TimeTracker.Library.Models.WebReport;

namespace TimeTracker.Library.Services
{
    public class WebReportService
    {
        private readonly TimeTrackerDbContext db;
        public WebReportService(in TimeTrackerDbContext db)
        {
            this.db = db;
        }

        public async Task<IImmutableList<UserEntry>> GetUserReport(Guid userId)
        {
            var timeEntries = await db.TimeEntries.Where(x => x.UserId == userId).ToListAsync();

            
            var query = from u in timeEntries
                group u by u.Date
                into g
                select new UserEntry
                {
                    UserId = g.FirstOrDefault().UserId,
                    Date = g.FirstOrDefault().Date,
                    BillableHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.BillableProject).Sum(x=>x.Hours),
                    SickHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick).Sum(x=>x.Hours),
                    VacationHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation).Sum(x=>x.Hours),
                    OtherNonBillable = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable).Sum(x=>x.Hours)
                };
            return query.ToImmutableList();
        }
    }
}