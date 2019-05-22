using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
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
            var timeEntries = await db.TimeEntries.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
            if (!timeEntries.Any())
            {
                return new ImmutableArray<UserEntry>();
            }
            var user = db.Users.First(x => x.UserId == userId);
            var name = user.FirstName + " " + user.LastName;
            
            var query = from u in timeEntries
                group u by u.Date
                into g
                select new UserEntry
                {
                    UserId = g.FirstOrDefault().UserId,
                    Name = name,
                    Date = g.FirstOrDefault().Date.ToShortDateString(),
                    DateForOrdering = g.FirstOrDefault().Date,
                    DayOfWeek = g.FirstOrDefault().Date.DayOfWeek.ToString(),
                    BillableHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.BillableProject).Sum(x=>x.Hours),
                    BillableProject = db.Projects.FirstOrDefault(x => x.ProjectId == (g.FirstOrDefault(y=>y.TimeEntryType == TimeEntryTypeEnum.BillableProject).ProjectId ?? 0))?.Name ?? "",
                    SickHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick).Sum(x=>x.Hours),
                    SickReason = g.FirstOrDefault(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick)?.NonBillableReason ?? "",
                    VacationHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation).Sum(x=>x.Hours),
                    VacationReason = g.FirstOrDefault(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation)?.NonBillableReason ?? "",
                    OtherNonBillable = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable).Sum(x=>x.Hours),
                    NonBillableReason = g.FirstOrDefault(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable)?.NonBillableReason ?? "",
                    TotalHours = g.Sum(x=>x.Hours)
                };
            return query.OrderByDescending(x => x.DateForOrdering).ToImmutableList();
        }
    }
}