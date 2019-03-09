using System;

namespace TimeTracker.Library.Models
{
    public class TimeEntry
    {
        public double Hours { get; set; }
        public String Project { get; set; }
        public DateTime Date { get; set; }
        /// <summary>
        /// could use something like this to determine if data already exists?
        /// </summary>
        public String Hash { get; set; }
    }
}