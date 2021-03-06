using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services
{
    public class UserReportService
    {
        private readonly TimeTrackerDbContext db;
        private readonly Guid userId;

        public UserReportService(TimeTrackerDbContext db, in Guid userId)
        {
            this.db = db;
            this.userId = userId;
        }

        public async Task<IReadOnlyCollection<HourPair>> QueryAllHours()
        {
            var timeEntries = await db.TimeEntries
                .Include(x => x.Project)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var allHours = (from t in timeEntries
                select new HourPair()
                {
                    Hours = t.Hours,
                    ProjectOrName = t.ProjectId.HasValue ? t.Project.Name : t.NonBillableReason,
                    Date = t.Date,
                    TimeEntryType = t.TimeEntryType
                }).ToList();

            return allHours;
        }

        public async Task<TimeEntryReport> GetHoursSummaryDefaultWeekMonthAndYtd()
        {
            var currentDate = DateTime.UtcNow;
            var timeEntryReport = new TimeEntryReport();
            
            timeEntryReport = await BuildWeeklyTimeEntryReport(currentDate, timeEntryReport);

            timeEntryReport = await BuildMonthlyTimeEntryReport(currentDate.Month, currentDate.Year, timeEntryReport);
            timeEntryReport = await BuildYearlyTimeEntryReport(currentDate.Year, timeEntryReport);
           
            return timeEntryReport;
        }

 

        public async Task<TimeEntryReport> GetHoursSummaryYear(int year)
        {
            return await BuildYearlyTimeEntryReport(year, new TimeEntryReport());
        }

        public async Task<TimeEntryReport> GetHoursSummaryMonth(int month, int year)
        {
            return await BuildMonthlyTimeEntryReport(month, year, new TimeEntryReport());
        }
        
        public async Task<TimeEntryReport> GetHoursSummaryForDay(DateTime date)
        {
            TimeEntryReport timeEntryReport = new TimeEntryReport();
            var allHours = await QueryAllHours();

            timeEntryReport.CurrentDayDisplay = date.ToString("MMMM d yyyy");
            timeEntryReport.BillableHoursDay = CalculateDailyHours(allHours, date, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursDay = CalculateDailyHours(allHours, date, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursDay = CalculateDailyHours(allHours, date, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursDay = CalculateDailyHours(allHours, date, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }

        private async Task<TimeEntryReport> BuildYearlyTimeEntryReport(int year, TimeEntryReport timeEntryReport)
        {
            var currentBeginningYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var allHours = await QueryAllHours();

            timeEntryReport.Year = year.ToString();
            timeEntryReport.BillableHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }

        private async Task<TimeEntryReport> BuildMonthlyTimeEntryReport(int month, int year, TimeEntryReport timeEntryReport)
        {
            var currentBeginningMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            timeEntryReport.CurrentMonthDisplay = currentBeginningMonth.ToString("MMMM yyyy");

            var allHours = await QueryAllHours();

            timeEntryReport.BillableHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }
        
        private async Task<TimeEntryReport> BuildWeeklyTimeEntryReport(DateTime currentDate, TimeEntryReport timeEntryReport)
        {
            var currentDayOfWeek = (int)currentDate.DayOfWeek;
            var beginningOfWeek = currentDate.AddDays(currentDayOfWeek * -1);
            var endOfWeek = beginningOfWeek.AddDays(7);

            timeEntryReport.CurrentWeekDisplay = "This Week";
            
            var allHours = await QueryAllHours();

            timeEntryReport.BillableHoursWeekly = CalculateWeeklyHours(allHours, beginningOfWeek, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursWeekly = CalculateWeeklyHours(allHours, beginningOfWeek, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursWeekly = CalculateWeeklyHours(allHours, beginningOfWeek, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursWeekly = CalculateWeeklyHours(allHours, beginningOfWeek, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;

        }
        
        public async Task<String> GetLastTenEntries()
        {
            var allHours = await QueryAllHours();
            var sortedHours = allHours.OrderByDescending(x => x.Date).ToList().Take(10);
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("The Last Ten Time Entries: ");

            foreach (var entry in sortedHours)
            {
                stringBuilder.AppendLine(entry.Date.Month + "-" + entry.Date.Day + "-" + entry.Date.Year + " " +
                                         entry.TimeEntryType.GetDescription() + " " + entry.Hours + " hours " + 
                                         entry.ProjectOrName);
            }

            return stringBuilder.ToString();

        }

        private double CalculateWeeklyHours(IReadOnlyCollection<HourPair> hours, DateTime beginningOfWeek, TimeEntryTypeEnum type)
        {
            var query = hours.Where(x => x.TimeEntryType == type);
            var endDate = beginningOfWeek.AddDays(7);

            query = query.Where(x => x.Date >= beginningOfWeek && x.Date < endDate);
            
            return query.Sum(x => x.Hours);
        }


        private double CalculateYearlyHours(IReadOnlyCollection<HourPair> hours, DateTime start, TimeEntryTypeEnum type)
        {
            var query = hours.Where(x => x.TimeEntryType == type);
            DateTime endDate = new DateTime(start.Year + 1, 1, 1);

            query = query.Where(x => x.Date >= start && x.Date < endDate);

            return query.Sum(x => x.Hours);
        }

        private double CalculateMonthlyHours(IReadOnlyCollection<HourPair> hours, DateTime start, TimeEntryTypeEnum type)
        {
            var query = hours.Where(x => x.TimeEntryType == type);
            DateTime endDate;
            if (start.Month == 12)
            {
                endDate = new DateTime(start.Year + 1, 1, 1);
            }
            else
            {
                endDate = new DateTime(start.Year, start.Month + 1, 1);
            }

            query = query.Where(x => x.Date >= start && x.Date < endDate);

            return query.Sum(x => x.Hours);
        }
        
        private double CalculateDailyHours(IReadOnlyCollection<HourPair> hours, DateTime date, TimeEntryTypeEnum type)
        {
            var query = hours.Where(x => x.TimeEntryType == type);
            query = query.Where(x => x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year);

            return query.Sum(x => x.Hours);        
        }

     
    }
}