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

        public async Task<TimeEntryReport> GetHoursSummaryMonthAndYtd(int? month)
        {
            var currentDate = DateTime.UtcNow;
            if (!month.HasValue)
            {
                month = currentDate.Month;
            }
            DateTime currentBeginningMonth = new DateTime(currentDate.Year, month.Value, 1, 1, 1, 1, DateTimeKind.Utc);
            DateTime currentBeginningYear = new DateTime(currentDate.Year, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            TimeEntryReport timeEntryReport = new TimeEntryReport();
            
            timeEntryReport.CurrentMonthDisplay = currentBeginningMonth.ToString("MMMM yyyy");

            var allHours = await this.QueryAllHours();

            timeEntryReport.BillableHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.BillableProject);
            timeEntryReport.BillableHourssYtd = CalculateHours(allHours, currentBeginningYear, TimeEntryTypeEnum.BillableProject);

            timeEntryReport.SickHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick); 
            timeEntryReport.VacationHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            timeEntryReport.NonBillableHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.NonBillable); 
            
            timeEntryReport.SickHoursYtd = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick); 
            timeEntryReport.VacationHoursYtd = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            timeEntryReport.NonBillableHoursYtd = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.NonBillable);

            return timeEntryReport;
        }
        
        private double CalculateHours(IReadOnlyCollection<HourPair> hours, DateTime? start, TimeEntryTypeEnum type)
        {
            var query = hours.Where(x => x.TimeEntryType == type);

            if (start.HasValue)
            {
                query = query.Where(x => x.Date >= start);
            }
            
            return query.Sum(x=>x.Hours);
        }
    }
}