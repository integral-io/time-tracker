using System.Collections.Generic;

namespace TimeTracker.Api.Models
{
    public class AllTimeOffDto
    {
        public List<TimeOffDto> TimeOffSummaries;
    }

    public class TimeOffDto
    {
        public string Username;
        public double SickYTD;
        public double PtoTYD;
    }
}