using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Data.Models;

namespace TimeTracker.Api.Models
{
    public class TimeEntryReportDto
    {
        public List<HourPairDto> ProjectHours { get; set; }

        // todo: unit test
        public string ToMessage()
        {
            DateTime currentBeginningMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 1, 1, 1, DateTimeKind.Utc);
            string currentMonthDisplay = currentBeginningMonth.ToString("MMM yyyy");
            
            double billableHoursMonth = ProjectHours.Where(x =>
                x.Date >= currentBeginningMonth && x.TimeEntryType == TimeEntryTypeEnum.BillableProject).Sum(x=>x.Hours);
            double sickHoursMonth = ProjectHours
                .Where(x => x.Date >= currentBeginningMonth && x.TimeEntryType == TimeEntryTypeEnum.Sick)
                .Sum(x => x.Hours);
            double vacationHoursMonth = ProjectHours
                .Where(x => x.Date >= currentBeginningMonth && x.TimeEntryType == TimeEntryTypeEnum.Vacation)
                .Sum(x => x.Hours);
            double nonBillableHoursMonth = ProjectHours
                .Where(x => x.Date >= currentBeginningMonth && x.TimeEntryType == TimeEntryTypeEnum.NonBillable)
                .Sum(x => x.Hours);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{currentMonthDisplay} Billable Hours: {billableHoursMonth:F1}");
            sb.AppendLine($"{currentMonthDisplay} Sick Hours: {sickHoursMonth:F1}");
            sb.AppendLine($"{currentMonthDisplay} Vacation Hours: {vacationHoursMonth:F1}");
            sb.AppendLine($"{currentMonthDisplay} Other Non-billable Hours: {nonBillableHoursMonth:F1}");
            sb.AppendLine("------------------------");
            sb.AppendLine("YTD Total Billable Hours:");
            sb.AppendLine("YTD Total Sick Hours: ");
            sb.AppendLine("YTD Total Vacation Hours:");
            sb.AppendLine("YTD Total Other Non-billable Hours:");

            return sb.ToString();
        }
    }

    public class HourPairDto
    {
        public string ProjectOrName { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public TimeEntryTypeEnum TimeEntryType { get; set; }
    }
}