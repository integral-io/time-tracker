using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Library.Models;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeEntry = TimeTracker.Data.Models.TimeEntry;

namespace TimeTracker.Library.Services
{
    public class TimeEntryService
    {
        private readonly Guid userId;
        private readonly TimeTrackerDbContext db;

        public TimeEntryService(in Guid userId, TimeTrackerDbContext db)
        {
            this.userId = userId;
            this.db = db;
        }

        public async Task<Guid> CreateBillableTimeEntry(DateTime date, double hours, int billableClientId, int projectId)
        {
            var model = new TimeEntry
            {
                TimeEntryId = Guid.NewGuid(),
                BillingClientId = billableClientId,
                IsBillable = true,
                Date = date,
                UserId = userId,
                ProjectId = projectId,
                Hours = hours
            };
            db.TimeEntries.Add(model);
            await db.SaveChangesAsync();

            db.DetachEntity(model);
            
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
                UserId = userId,
                NonBillableReason = nonBillReason,
                TimeEntryType = timeEntryTypeEnum
            };
            db.TimeEntries.Add(model);
            await db.SaveChangesAsync();
            
            db.DetachEntity(model);
            
            return model.TimeEntryId;
        }

        public async Task<double> DeleteHours(DateTime commandDtoDate)
        {
            var timeEntries = await db.TimeEntries.Where(x => x.UserId == userId && x.Date >= commandDtoDate.Date).ToListAsync();
            if (timeEntries == null || timeEntries.Count == 0)
            {
                return 0;
            }
            var hoursDeleted = timeEntries.Sum(x=>x.Hours);
            
            db.TimeEntries.RemoveRange(timeEntries);

            await db.SaveChangesAsync();
            
            timeEntries.ForEach(x=> db.DetachEntity(x));
            
            return hoursDeleted;
        }

        public async Task<AllTimeOff> QueryAllTimeOff()
        {
            var query = from t in db.TimeEntries
                group t by t.UserId into g
                select new TimeOff()
                {
                    Username = g.FirstOrDefault().User.UserName,
                    PtoTyd = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Vacation).Sum(x=>x.Hours),
                    SickYtd = g.Where(x=>x.TimeEntryType == TimeEntryTypeEnum.Sick).Sum(x=>x.Hours)
                };
            var hours = await query.AsNoTracking().ToListAsync();

            var allTimeOff = new AllTimeOff()
            {
                TimeOffSummaries = hours
            };
            
            return allTimeOff;
        }
    }

   
}