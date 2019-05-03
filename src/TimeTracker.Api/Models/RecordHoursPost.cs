using TimeTracker.Data.Models;

namespace TimeTracker.Api.Models
{
    public class RecordHoursPost
    {
        public int ProjectId { get; set; }
        public TimeEntryTypeEnum TimeEntryType { get; set; }
        public double Hours { get; set; }
        public string NonbillReason { get; set; }
        public string Date { get; set; }
    }
}