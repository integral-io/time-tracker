namespace TimeTracker.Library.Models
{
    public class TotalHourSummary
    {
        public double TotalBillable { get; set; }
        public double TotalSick { get; set; }
        public double TotalVacation { get; set; }
        public double TotalNonBillable { get; set; }
    }
}