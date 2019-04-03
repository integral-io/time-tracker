using System;
using TimeTracker.Data.Models;

namespace TimeTracker.Library.Models
{
    public class HourPair
    {
        public string ProjectOrName { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public TimeEntryTypeEnum TimeEntryType { get; set; }
    }
}