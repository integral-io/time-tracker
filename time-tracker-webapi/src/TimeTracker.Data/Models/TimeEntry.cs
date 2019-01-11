using System;

namespace TimeTracker.Data.Models
{
    public class TimeEntry
    {
        public Guid TimeEntryId { get; set; }

        public Guid UserId { get; set; }

        public string Project { get; set; }

        public double Hours { get; set; }

        public DateTime Date { get; set; }

        public Boolean IsBillable { get; set; }

        public int? BillingClientId { get; set; }

        public BillingClient BillingClient { get; set; }
        
    }
}