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
        private readonly TimeTrackerDbContext _db;
        private readonly Guid _userId;

        public UserReportService(TimeTrackerDbContext db, Guid userId)
        {
            _db = db;
            _userId = userId;
        }
        
        public async Task<IReadOnlyCollection<HourPairDto>> QueryAllHours()
        {
            var timeEntries = await _db.TimeEntries
                .Include(x => x.Project)
                .Where(x => x.UserId == _userId)
                .ToListAsync();

            var allHours = (from t in timeEntries
                select new HourPairDto()
                {
                    Hours = t.Hours,
                    ProjectOrName = t.ProjectId.HasValue ? t.Project.Name : null,
                    Date = t.Date,
                    TimeEntryType = t.TimeEntryType
                }).ToList();

            return allHours;
        }

        public async Task<dynamic> GetHoursSummary()
        {
            DateTime currentBeginningMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 1, 1, 1, DateTimeKind.Utc);
            DateTime currentBeginningYear = new DateTime(DateTime.UtcNow.Year, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            
            string currentMonthDisplay = currentBeginningMonth.ToString("MMM yyyy");

            var allHours = await this.QueryAllHours();
            
            double billableHoursMonth = allHours.Where(x =>
                    x.Date >= currentBeginningMonth && x.TimeEntryType == TimeEntryTypeEnum.BillableProject)
                .Sum(x=>x.Hours);

            double billableHoursYtd = CalculateHours(allHours, currentBeginningYear, TimeEntryTypeEnum.BillableProject);

            double sickHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick); 
            double vacationHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            double nonBillableHoursMonth = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            
            
            double sickHoursYTD = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Sick); 
            double vacationHoursYTD = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            double nonBillableHoursYTD = CalculateHours(allHours, currentBeginningMonth, TimeEntryTypeEnum.Vacation);

            // todo: wip
            return new
            {
                currentMonthDisplay = currentMonthDisplay
            };
        }
        
        private double CalculateHours(IReadOnlyCollection<HourPairDto> hours, DateTime? start, TimeEntryTypeEnum type)
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