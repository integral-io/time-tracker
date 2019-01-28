using System;
using System.Threading.Tasks;
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

        public async Task<object> QueryHours(string commandDtoProject, DateTime commandDtoStartDateMonth)
        {
            throw new NotImplementedException();
        }
    }
}