using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using TimeTracker.Data.Models;

namespace TimeTracker.Api
{
    public static class SlackMessageInterpreter
    {
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
        /// <param name="text">ex: record au 8 wfh. or record au 8 yesterday, record vacation 8, record sick 6.</param>
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
            
            TimeEntryTypeEnum entryTypeEnum;
            if (TimeEntryTypeEnum.TryParse(dto.Project, true, out entryTypeEnum))
            {
                dto.TimeEntryType = entryTypeEnum;
            }
            else
            {
                dto.IsBillable = !text.Contains("nonbill");
                dto.TimeEntryType = TimeEntryTypeEnum.NonBillable;
            }

            if (!dto.IsBillable)
            {
                int startIndexOfReason = text.IndexOf("\"", StringComparison.Ordinal);
                if (startIndexOfReason > 0)
                {
                    dto.NonBillReason = text.Substring(startIndexOfReason).Trim('"');
                }
            }
            
            // process date portion
            string datePortion = splitText.Length >= 4 ? splitText[3] : null;
            
            // todo: move all of this logic to EasyDateParser
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
                DateTime? easyDate = EasyDateParser.ParseEasyDate(datePortion);
                dto.Date = easyDate ?? EasyDateParser.GetUtcNow();
            }

            return dto;
        }


        public static DeleteInterpretedCommandDto InterpretDeleteMessage(string text)
        {
            string[] splitText = text.ToLowerInvariant().Split(' ');
            if (!text.StartsWith("delete"))
            {
                return new DeleteInterpretedCommandDto()
                    {ErrorMessage = $"Invalid start option: {splitText.FirstOrDefault()}"};
            }
            var dto = new DeleteInterpretedCommandDto();
            string datePortion = splitText.Length >= 2 ? splitText[1] : null;
            
            if (DateTime.TryParse(datePortion, out var dateTime))
            {
                dto.Date = dateTime;
            }
            else
            {
                DateTime? easyDate = EasyDateParser.ParseEasyDate(datePortion);
                dto.Date = easyDate ?? EasyDateParser.GetUtcNow();
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

        public TimeEntryTypeEnum TimeEntryType { get; set; }
    }

    public class ReportInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
        public DateTime StartDateMonth { get; set; }
    }

    public class DeleteInterpretedCommandDto : CommandDtoBase
    {
        public DateTime Date { get; set; }
    }

    public class CommandDtoBase
    {
        public string ErrorMessage { get; set; }
    }
}