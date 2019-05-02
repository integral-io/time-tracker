using System;
using System.Collections.Immutable;

namespace TimeTracker.Library.Models.Admin
{
    public class PayPeriodReportViewModel
    {
        public DateTime PayPeriodStartDate { get; set; }
        public DateTime PayPeriodEndDate { get; set; }
        public ImmutableList<UserReport> ReportItems { get; set; }
    }
}