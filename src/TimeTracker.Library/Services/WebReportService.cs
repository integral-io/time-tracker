using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
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


        public async Task<ImmutableList<SelectListItem>> GetUserAvailableMonths(Guid userId)
        {
            var allMonthAndYears =
                from t in await db.TimeEntries.AsNoTracking().Where(x => x.UserId == userId).ToListAsync()
                group t by t.Date
                into g
                select new
                {
                    MonthAndYear = $"{g.Key.Date.Year}-{g.Key.Date.Month}"
                };

            var final = allMonthAndYears.GroupBy(x => x.MonthAndYear);

            return (ImmutableList<SelectListItem>) final.Select(x => new SelectListItem(x.Key, $"{x.Key}-01"));
        }
        
        /// <summary>
        /// gets user's report with all entries for the selected month, or current month if none is selected
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<IImmutableList<UserEntry>> GetUserReport(Guid userId, int? month = null)
        {
            var timeEntries = await db.TimeEntries.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
            if (!timeEntries.Any())
            {
                return new ImmutableArray<UserEntry>();
            }
            var user = db.Users.First(x => x.UserId == userId);
            var name = user.FirstName + " " + user.LastName;
            int currentMonth = DateTime.UtcNow.Month;

            var testData = timeEntries.ToList();

            var query = from u in timeEntries.Where(x=> x.Date.Month == (month ?? currentMonth))
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