using System;
using System.Linq;

namespace TimeTracker.Api
{
    public static class SlackMessageInterpreter
    {
        public const string OPTION_RECORD = "record";
        public const string OPTION_REPORT = "report";

        /// <summary>
        /// parse command string for report
        /// </summary>
        /// <param name="text">ex: report au, report, report au 2018-12</param>
        /// <returns></returns>
        public static ReportInterpretedCommandDto InterpretReportMessage(string text)
        {
            string[] splitText = text.ToLowerInvariant().Split(' ');
            if (!text.StartsWith("report"))
            {
                return new ReportInterpretedCommandDto()
                    {ErrorMessage = $"Invalid start option: {splitText.FirstOrDefault()}"};
            }

            var dto = new ReportInterpretedCommandDto();
            if (splitText.Length > 1)
            {
                dto.Project = splitText[1];
            }
            string datePortion = splitText[splitText.Length-1];
            if (datePortion.Contains("-"))
            {
                string[] splitDate = datePortion.Split('-');
                int year = Convert.ToInt32(splitDate[0]);
                int month = Convert.ToInt32(splitDate[1]);
                dto.StartDateMonth = new DateTime(year, month, 1,0,0,0,DateTimeKind.Utc);
            }
            else
            {
                var utcNow = DateTime.UtcNow;
                dto.StartDateMonth = new DateTime(utcNow.Year, utcNow.Month, 1,0,0,0,DateTimeKind.Utc);
            }
            
            return dto;
        }

        /// <summary>
        /// parse command string and return typed object
        /// </summary>
        /// <param name="text">ex: record au 8 wfh. or record au 8 yesterday.</param>
        /// <returns></returns>
        public static HoursInterpretedCommandDto InterpretHoursRecordMessage(string text)
        {
            string[] splitText = text.ToLowerInvariant().Split(' ');
            if (!text.StartsWith("record"))
            {
                return new HoursInterpretedCommandDto()
                    {ErrorMessage = $"Invalid start option: {splitText.FirstOrDefault()}"};
            }

            var dto = new HoursInterpretedCommandDto();
            dto.Project = splitText[1];
            dto.IsWorkFromHome = text.Contains("wfh");
            dto.Hours = Convert.ToDouble(splitText[2]);
            dto.IsBillable = !text.Contains("nonbill");

            if (!dto.IsBillable)
            {
                dto.NonBillReason = text.Substring(text.IndexOf("\"")).Trim('"');
            }
            
            // process date portion
            string datePortion = splitText.Length >= 4 ? splitText[3] : null;

            if (datePortion == "yesterday")
            {
                dto.Date = DateTime.UtcNow.AddDays(-1);
            }
            else if (DateTime.TryParse(datePortion, out var dateTime))
            {
                dto.Date = dateTime;
            }
            else
            {
                dto.Date = DateTime.UtcNow;
            }

            return dto;
        }
    }

    public class HoursInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public bool IsWorkFromHome { get; set; }
        public bool IsBillable { get; set; }
        public string NonBillReason { get; set; }
    }

    public class ReportInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
        public DateTime StartDateMonth { get; set; }
    }

    public class CommandDtoBase
    {
        public string ErrorMessage { get; set; }
    }
}