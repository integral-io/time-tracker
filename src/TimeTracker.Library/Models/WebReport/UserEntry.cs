using System;

namespace TimeTracker.Library.Models.WebReport
{
    public class UserEntry
    {
        public Guid UserId { get; set; }
        
        public string Name { get; set; }
        public string Date { get; set; }
        public string DayOfWeek { get; set; }

        public double BillableHours { get; set; }

        //public string BillableProject { get; set; }

        public double SickHours { get; set; }

        public string SickReason { get; set; }

        public double VacationHours { get; set; }
        
        public string VacationReason { get; set; }
        public double OtherNonBillable { get; set; }
        
        public string NonBillableReason { get; set; }
    }
}