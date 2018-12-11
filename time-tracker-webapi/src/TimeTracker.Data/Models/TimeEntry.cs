using System;

namespace TimeTracker.Data.Models
{
    public class TimeEntry
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string Project { get; set; }

        public string Client { get; set; }
        
        public double Hours { get; set; }

        public DateTime Date { get; set; }

        public Boolean IsBillable { get; set; }
        
    }
}