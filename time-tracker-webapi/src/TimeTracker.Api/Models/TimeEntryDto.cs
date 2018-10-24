using System;

namespace TimeTracker.Api.Models
{
    public class TimeEntryDto
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