using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Models;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Api.Services
{
    public class TimeEntryService
    {
        private readonly Guid _userId;
        private readonly TimeTrackerDbContext _db;

        public TimeEntryService(Guid userId, TimeTrackerDbContext db)
        {
            _userId = userId;
            _db = db;
        }

        public async Task<Guid> CreateBillableTimeEntry(DateTime date, double hours, int billableClientId, int projectId)
        {
            var model = new TimeEntry
            {
                TimeEntryId = Guid.NewGuid(),
                BillingClientId = billableClientId,
                IsBillable = true,
                Date = date,
                UserId = _userId,
                ProjectId = projectId,
                Hours = hours
            };
            _db.TimeEntries.Add(model);
            await _db.SaveChangesAsync();
            
            return model.TimeEntryId;
        }
        
        public async Task<Guid> CreateNonBillableTimeEntry(DateTime date, double hours, string nonBillReason, 
            TimeEntryTypeEnum timeEntryTypeEnum = TimeEntryTypeEnum.NonBillable)
        {
            var model = new TimeEntry
            {
                TimeEntryId = Guid.NewGuid(),
                IsBillable = false,
                Date = date,
                Hours = hours,
                UserId = _userId,
                NonBillableReason = nonBillReason,
                TimeEntryType = timeEntryTypeEnum
            };
            _db.TimeEntries.Add(model);
            await _db.SaveChangesAsync();
            
            return model.TimeEntryId;
        }

        // todo: unit test
        public async Task<TimeEntryReportDto> QueryHours(DateTime startDateMonth)
        {
            DateTime beginningCurrentYear = new DateTime(DateTime.UtcNow.Year, 1, 1, 1,1,1,DateTimeKind.Utc);
            var timeEntries = await _db.TimeEntries.Include(x=>x.Project).Where(x => x.UserId == _userId && x.Date >= beginningCurrentYear)
                .ToListAsync();
            var report = new TimeEntryReportDto()
            {
                ProjectHours = (from t in timeEntries
                                select new HourPairDto()
                                {
                                    Hours = t.Hours,
                                    ProjectOrName = t.Project.Name,
                                    Date = t.Date,
                                    TimeEntryType = t.TimeEntryType
                                }).ToList()
            };

            return report;
        }

        public async Task<double> DeleteHours(DateTime commandDtoDate)
        {
            var timeEntries = await _db.TimeEntries.Where(x => x.UserId == _userId && x.Date >= commandDtoDate.Date).ToListAsync();
            if (timeEntries == null || timeEntries.Count == 0)
            {
                return 0;
            }
            var hoursDeleted = timeEntries.Sum(x=>x.Hours);
            
            _db.TimeEntries.RemoveRange(timeEntries);

            await _db.SaveChangesAsync();
            return hoursDeleted;
        }
    }
}