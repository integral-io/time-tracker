using System;

namespace TimeTracker.Data.Models
{
    /// <summary>
    /// This defines the billing rate for a user and client, optionally with a timespan for the rate
    /// </summary>
    public class BillingRate
    {
        public int BillingRateId { get; set; }
        public Guid UserId { get; set; }
        public int BillingClientId { get; set; }
        public int? ProjectId { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }
}