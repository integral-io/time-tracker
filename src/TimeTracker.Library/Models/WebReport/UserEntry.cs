using System;

namespace TimeTracker.Library.Models.WebReport
{
    public class UserEntry
    {
        public Guid UserId { get; set; }

        public string Date { get; set; }

        public double BillableHours { get; set; }
        
        public double SickHours { get; set; }

        public double VacationHours { get; set; }

        public double OtherNonBillable { get; set; }
        
    }
}