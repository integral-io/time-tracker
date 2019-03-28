using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Data.Models;

namespace TimeTracker.Library.Models
{
    public class TimeEntryReport
    {
        public string ToMessage()
        {
            var sb = new StringBuilder();
            // todo: count of billable entries
            sb.AppendLine($"{this.CurrentMonthDisplay} Billable Hours: {this.BillableHoursMonth:F1}");
            sb.AppendLine($"{this.CurrentMonthDisplay} Sick Hours: {this.SickHoursMonth:F1}");
            sb.AppendLine($"{this.CurrentMonthDisplay} Vacation Hours: {this.VacationHoursMonth:F1}");
            sb.AppendLine($"{this.CurrentMonthDisplay} Other Non-billable Hours: {this.NonBillableHoursMonth:F1}");
            sb.AppendLine("------------------------");
            sb.AppendLine($"YTD Total Billable Hours: {this.BillableHourssYtd:F1}");
            sb.AppendLine($"YTD Total Sick Hours: {this.SickHoursYtd:F1}");
            sb.AppendLine($"YTD Total Vacation Hours: {this.VacationHoursYtd:F1}");
            sb.AppendLine($"YTD Total Other Non-billable Hours: {this.NonBillableHoursYtd:F1}");

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