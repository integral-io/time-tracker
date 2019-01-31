using System;

namespace TimeTracker.Data.Models
{
    public class TimeEntry
    {
        public Guid TimeEntryId { get; set; }

        public double Hours { get; set; }

        /// <summary>
        /// Always saves date as UTC in db. We dont care about date conversions, always assume fixed date/tz for users.
        /// </summary>
        public DateTime Date { get; set; }

        public Boolean IsBillable { get; set; }
        public string NonBillableReason { get; set; }

        public int? BillingClientId { get; set; }
        public int? ProjectId { get; set; }
        public Guid UserId { get; set; }

        #region Navigation Properties
        
        public BillingClient BillingClient { get; set; }
        public User User { get; set; }
        public Project Project { get; set; }
        public TimeEntryTypeEnum TimeEntryType { get; set; }
        
        #endregion
    }
}