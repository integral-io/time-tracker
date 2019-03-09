using System.Collections.Generic;

namespace TimeTracker.Library.Models
{
    public class AllTimeOff
    {
        public List<TimeOff> TimeOffSummaries;
    }

    public class TimeOff
    {
        public string Username;
        public double SickYtd;
        public double PtoTyd;
    }
}