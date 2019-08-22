using System;
using System.Collections.Generic;

namespace TimeTracker.Library.Models.WebReport
{
    /// <summary>
    /// presentation object for the user's web report
    /// </summary>
    public class UserEntry
    {
        public Guid UserId { get; set; }
        public DateTime DateForOrdering { get; set; }
        
        public string Name { get; set; }
        public string Date { get; set; }
        public string DayOfWeek { get; set; }

        public List<ProjectHours> BillableHours { get; set; }

        public double SickHours { get; set; }

        public string SickReason { get; set; }

        public double VacationHours { get; set; }
        
        public string VacationReason { get; set; }

        public List<ProjectHours> NonBillableHours { get; set; }
        
        public double TotalHours { get; set; }
    }

    /// <summary>
    /// defines underlying billable hour to use as a list
    /// </summary>
    public class ProjectHours
    {
        public double Hours { get; set; }

        public string Project { get; set; }
    }
}