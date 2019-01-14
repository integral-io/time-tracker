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

        // todo: get _db from Services
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
                ProjectId = projectId
            };
            _db.TimeEntries.Add(model);
            await _db.SaveChangesAsync();
            
            return model.TimeEntryId;
        }
        
        public async Task<Guid> CreateNonBillableTimeEntry(DateTime date, double hours, string nonBillReason)
        {
            var model = new TimeEntry
            {
                TimeEntryId = Guid.NewGuid(),
                IsBillable = false,
                Date = date,
                UserId = _userId,
                NonBillableReason = nonBillReason,
            };
            _db.TimeEntries.Add(model);
            await _db.SaveChangesAsync();
            
            return model.TimeEntryId;
        }
    }
}