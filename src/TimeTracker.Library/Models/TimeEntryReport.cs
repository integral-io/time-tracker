using System.Text;

namespace TimeTracker.Library.Models
{
    public class TimeEntryReport
    {
        public string ToMonthAndYTDMessage()
        {
            return MonthlyMessageBuilder()
                .AppendLine("------------------------")
                .Append(YearlyMessageBuilder())
                .ToString();
        }
        public string ToDayMessage()
        {
            return new StringBuilder()
                .AppendLine($"{CurrentDayDisplay} Billable Hours: {BillableHoursDay:F1}")
                .AppendLine($"{CurrentDayDisplay} Sick Hours: {SickHoursDay:F1}")
                .AppendLine($"{CurrentDayDisplay} Vacation Hours: {VacationHoursDay:F1}")
                .AppendLine($"{CurrentDayDisplay} Other Non-billable Hours: {NonBillableHoursDay:F1}")
                .ToString();
        }

        public string ToMonthlyMessage()
        {
            return MonthlyMessageBuilder().ToString();
        }

        public string ToYearlyMessage()
        {
            return YearlyMessageBuilder().ToString();
        }

        
        private StringBuilder MonthlyMessageBuilder()
        {
            return new StringBuilder()
                // todo: count of billable entries
                .AppendLine($"{CurrentMonthDisplay} Billable Hours: {BillableHoursMonth:F1}")
                .AppendLine($"{CurrentMonthDisplay} Sick Hours: {SickHoursMonth:F1}")
                .AppendLine($"{CurrentMonthDisplay} Vacation Hours: {VacationHoursMonth:F1}")
                .AppendLine($"{CurrentMonthDisplay} Other Non-billable Hours: {NonBillableHoursMonth:F1}");
        }

        private StringBuilder YearlyMessageBuilder()
        {
            return new StringBuilder()
                .AppendLine($"{Year} Total Billable Hours: {BillableHoursYtd:F1}")
                .AppendLine($"{Year} Total Sick Hours: {SickHoursYtd:F1}")
                .AppendLine($"{Year} Total Vacation Hours: {VacationHoursYtd:F1}")
                .AppendLine($"{Year} Total Other Non-billable Hours: {NonBillableHoursYtd:F1}");
        }

        public double NonBillableHoursDay { get; set; }

        public double VacationHoursDay { get; set; }

        public double SickHoursDay { get; set; }

        public double BillableHoursDay { get; set; }
        public double NonBillableHoursYtd { get; set; }

        public double VacationHoursYtd { get; set; }

        public double SickHoursYtd { get; set; }

        public double BillableHoursYtd { get; set; }

        public double NonBillableHoursMonth { get; set; }

        public double VacationHoursMonth { get; set; }

        public double SickHoursMonth { get; set; }

        public double BillableHoursMonth { get; set; }
        public string CurrentDayDisplay { get; set; }

        public string CurrentMonthDisplay { get; set; }

        public string Year { get; set; }
    }
}