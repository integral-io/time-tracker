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
            VerifyHoursBeforeAdding(date, hours);

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
            VerifyHoursBeforeAdding(date, hours);

            var offTime = db.TimeEntries.Where(x => x.UserId == userId && x.Date.Date == date.Date.Date && 
                                                         (x.TimeEntryType == TimeEntryTypeEnum.Vacation || 
                                                          x.TimeEntryType == TimeEntryTypeEnum.Sick)).Sum(x => x.Hours);
            if (hours + offTime > 8 && (timeEntryTypeEnum == TimeEntryTypeEnum.Vacation || timeEntryTypeEnum == TimeEntryTypeEnum.Sick))
            {
                throw new Exception("Cannot have more than 8 hours of combined vacation and sick time in a single day.");
            }
            
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

        private void VerifyHoursBeforeAdding(DateTime date, double hours)
        {
            if (hours <= 0)
            {
                throw new Exception("An entry should have more than 0 hours.");
            }

            var hoursForDay = db.TimeEntries.Where(x => x.UserId == userId && x.Date.Date == date.Date.Date).Sum(x => x.Hours);
            if (hoursForDay + hours > 24)
            {
                throw new Exception("You may not enter more than 24 hours per day.");
            }
        }

        public async Task<double> DeleteHours(DateTime date)
        {
            var cutOffDate = CheckCutOffDate(date);
            
            var timeEntries = await db.TimeEntries.Where(x => x.UserId == userId && 
                                                              x.Date.Date == date.Date.Date && 
                                                              x.Date >= cutOffDate).ToListAsync();

            if (timeEntries.Count == 0)
            {
                return 0;
            }
            
            return await DeleteHoursFromDb(timeEntries);
        }

        public async Task<double> DeleteHoursForTimeEntryType(DateTime date, TimeEntryTypeEnum timeEntryType)
        {
            var cutOffDate = CheckCutOffDate(date);

            var timeEntries = await db.TimeEntries.Where(x => x.UserId == userId &&
                                                              x.Date.Date == date.Date.Date &&
                                                              x.Date >= cutOffDate &&
                                                              x.TimeEntryType == timeEntryType).ToListAsync();

            if (timeEntries.Count == 0)
            {
                return 0;
            }
            
            return await DeleteHoursFromDb(timeEntries);
        }

        private async Task<double> DeleteHoursFromDb(List<TimeEntry> timeEntries)
        {
            var hoursDeleted = timeEntries.Sum(x => x.Hours);

            db.TimeEntries.RemoveRange(timeEntries);

            await db.SaveChangesAsync();

            timeEntries.ForEach(x => db.DetachEntity(x));
            return hoursDeleted;
        }

        private static DateTime CheckCutOffDate(DateTime date)
        {
            var cutOffDate = DateTime.UtcNow.Date.AddHours(-48);
            if (date < cutOffDate)
            {
                throw new Exception("Entries older than 48 hours cannot be deleted.");
            }

            return cutOffDate;
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