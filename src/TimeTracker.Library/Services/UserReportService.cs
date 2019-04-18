using System;
using System.Collections.Generic;
using System.Linq;
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
                    ProjectOrName = t.ProjectId.HasValue ? t.Project.Name : string.Empty,
                    Date = t.Date,
                    TimeEntryType = t.TimeEntryType
                }).ToList();

            return allHours;
        }

        public async Task<TimeEntryReport> GetHoursSummaryDefaultMonthAndYtd()
        {
            var currentDate = DateTime.UtcNow;
            var currentBeginningMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var currentBeginningYear = new DateTime(currentDate.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeEntryReport = new TimeEntryReport();

            timeEntryReport.CurrentMonthDisplay = currentBeginningMonth.ToString("MMMM yyyy");

            var allHours = await QueryAllHours();

            timeEntryReport.BillableHoursMonth =  CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.NonBillable);

            timeEntryReport.Year = currentBeginningYear.Year.ToString();
            timeEntryReport.BillableHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }

        public async Task<TimeEntryReport> GetHoursSummaryYear(int year)
        {
            var currentBeginningYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeEntryReport = new TimeEntryReport();

            var allHours = await QueryAllHours();

            timeEntryReport.Year = year.ToString();
            timeEntryReport.BillableHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursYtd = CalculateYearlyHours(allHours, currentBeginningYear, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }

        public async Task<TimeEntryReport> GetHoursSummaryMonth(int month)
        {
            var currentDate = DateTime.UtcNow;
            var currentBeginningMonth = new DateTime(currentDate.Year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeEntryReport = new TimeEntryReport();

            timeEntryReport.CurrentMonthDisplay = currentBeginningMonth.ToString("MMMM yyyy");

            var allHours = await QueryAllHours();

            timeEntryReport.BillableHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.SickHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick);
            timeEntryReport.VacationHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation);
            timeEntryReport.NonBillableHoursMonth = CalculateMonthlyHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }

        private double CalculateYearlyHours(IReadOnlyCollection<HourPair> hours, DateTime start, TimeEntryTypeEnum type)
        {
            var query = hours.Where(x => x.TimeEntryType == type);
            DateTime endDate = new DateTime(start.Year + 1, 1, 1);

            query = query.Where(x => x.Date >= start && x.Date < endDate);

            return query.Sum(x => x.Hours);
        }

        private double CalculateMonthlyHours(IReadOnlyCollection<HourPair> hours, DateTime start,
            TimeEntryTypeEnum type)
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
    }
}