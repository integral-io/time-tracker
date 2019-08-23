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

            var final = allMonthAndYears.GroupBy(x => x.MonthAndYear).OrderByDescending(x=>x.Key);

            return final.Select(x => new SelectListItem(x.Key, $"{x.Key}-01")).ToImmutableList();
        }
        
        /// <summary>
        /// gets user's report with all entries for the selected month, or current month if none is selected
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<IImmutableList<UserEntry>> GetUserReport(Guid userId, int? month = null)
        {
            var timeEntries = await db.TimeEntries.AsNoTracking()
                                        .Include(x=>x.Project)
                                        .Where(x => x.UserId == userId).ToListAsync();
            if (!timeEntries.Any())
            {
                return ImmutableArray.Create<UserEntry>();
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
                    BillableHours = (from bh in g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.BillableProject)
                                    select new ProjectHours()
                                    {
                                        Hours = bh.Hours,
                                        Project = bh.Project.Name
                                    }).ToList(),
                    SickHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick).Sum(x=>x.Hours),
                    SickReason = g.FirstOrDefault(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick)?.NonBillableReason ?? "",
                    VacationHours = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation).Sum(x=>x.Hours),
                    VacationReason = g.FirstOrDefault(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation)?.NonBillableReason ?? "",
                    NonBillableHours = (from bh in g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.NonBillable)
                                        select new ProjectHours()
                                        {
                                            Hours = bh.Hours,
                                            Project = bh.NonBillableReason
                                        }).ToList(),
                    TotalHours = g.Sum(x=>x.Hours)
                };
            return query.OrderByDescending(x => x.DateForOrdering).ToImmutableList();
        }
    }
}