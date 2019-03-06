using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Data.Models;

namespace TimeTracker.Library.Models
{
    public class TimeEntryReportDto
    {
        public List<HourPairDto> ProjectHours { get; set; }

        // todo: unit test
        public string ToMessage()
        {
            DateTime currentBeginningMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 1, 1, 1, DateTimeKind.Utc);
            DateTime currentBeginningYear = new DateTime(DateTime.UtcNow.Year, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            
            string currentMonthDisplay = currentBeginningMonth.ToString("MMM yyyy");
            
            double billableHoursMonth = ProjectHours.Where(x =>
                x.Date >= currentBeginningMonth && x.TimeEntryType == TimeEntryTypeEnum.BillableProject)
                .Sum(x=>x.Hours);

            double billableHourssYTD = CalculateHours(currentBeginningYear, TimeEntryTypeEnum.BillableProject);

            double sickHoursMonth = CalculateHours(currentBeginningMonth, TimeEntryTypeEnum.Sick); 
            double vacationHoursMonth = CalculateHours(currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            double nonBillableHoursMonth = CalculateHours(currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            
            
            double sickHoursYTD = CalculateHours(currentBeginningMonth, TimeEntryTypeEnum.Sick); 
            double vacationHoursYTD = CalculateHours(currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            double nonBillableHoursYTD = CalculateHours(currentBeginningMonth, TimeEntryTypeEnum.Vacation); 
            
            StringBuilder sb = new StringBuilder();
            // todo: count of billable entries
            sb.AppendLine($"{currentMonthDisplay} Billable Hours: {billableHoursMonth:F1}");
            sb.AppendLine($"{currentMonthDisplay} Sick Hours: {sickHoursMonth:F1}");
            sb.AppendLine($"{currentMonthDisplay} Vacation Hours: {vacationHoursMonth:F1}");
            sb.AppendLine($"{currentMonthDisplay} Other Non-billable Hours: {nonBillableHoursMonth:F1}");
            sb.AppendLine("------------------------");
            sb.AppendLine($"YTD Total Billable Hours: {billableHourssYTD:F1}");
            sb.AppendLine($"YTD Total Sick Hours: {sickHoursYTD:F1}");
            sb.AppendLine($"YTD Total Vacation Hours: {vacationHoursYTD:F1}");
            sb.AppendLine($"YTD Total Other Non-billable Hours: {nonBillableHoursYTD:F1}");

            return sb.ToString();
        }

        private double CalculateHours(DateTime? start, TimeEntryTypeEnum type)
        {
            var query = ProjectHours
                .Where(x => x.TimeEntryType == type);

            if (start.HasValue)
            {
                query = query.Where(x => x.Date >= start);
            }
            
            return query.Sum(x=>x.Hours);
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