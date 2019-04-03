using System.Text;

namespace TimeTracker.Library.Models
{
    public class TimeEntryReport
    {
        public string ToMessage()
        {
            var sb = new StringBuilder();
            // todo: count of billable entries
            sb.AppendLine($"{CurrentMonthDisplay} Billable Hours: {BillableHoursMonth:F1}");
            sb.AppendLine($"{CurrentMonthDisplay} Sick Hours: {SickHoursMonth:F1}");
            sb.AppendLine($"{CurrentMonthDisplay} Vacation Hours: {VacationHoursMonth:F1}");
            sb.AppendLine($"{CurrentMonthDisplay} Other Non-billable Hours: {NonBillableHoursMonth:F1}");
            sb.AppendLine("------------------------");
            sb.AppendLine($"YTD Total Billable Hours: {BillableHourssYtd:F1}");
            sb.AppendLine($"YTD Total Sick Hours: {SickHoursYtd:F1}");
            sb.AppendLine($"YTD Total Vacation Hours: {VacationHoursYtd:F1}");
            sb.AppendLine($"YTD Total Other Non-billable Hours: {NonBillableHoursYtd:F1}");

            return sb.ToString();
        }

        public double NonBillableHoursYtd { get; set; }

        public double VacationHoursYtd { get; set; }

        public double SickHoursYtd { get; set; }

        public double BillableHourssYtd { get; set; }

        public double NonBillableHoursMonth { get; set; }

        public double VacationHoursMonth { get; set; }

        public double SickHoursMonth { get; set; }

        public double BillableHoursMonth { get; set; }

        public string CurrentMonthDisplay { get; set; }
    }
}