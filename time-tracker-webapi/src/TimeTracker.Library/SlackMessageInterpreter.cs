using System;
using System.Linq;
using TimeTracker.Data.Models;

namespace TimeTracker.Library
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
            string datePortion = splitText.Length > 1 ? splitText[splitText.Length-1] : null;
            
            ProcessDate(datePortion,dto);
            
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
            if (dto.NonBillReason != null || dto.IsWorkFromHome)
            {
                string preprocessed = text.ToLowerInvariant()
                    .Replace("wfh","")
                    .Replace($"\"{dto.NonBillReason}\"", "");
                splitText = preprocessed.Split(' ');
            }

            string datePortion = splitText.Length >= 4 ? splitText[3] : null;
            
            ProcessDate(datePortion, dto);

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
            
            ProcessDate(datePortion, dto);

            return dto;
        }

        private static void ProcessDate(string datePortion, CommandDtoBase dto)
        {
            if (String.IsNullOrEmpty(datePortion))
            {
                dto.Date = EasyDateParser.GetUtcNow();
            }
            else
            {
                DateTime? easyDate = EasyDateParser.ParseEasyDate(datePortion);
                if (!easyDate.HasValue)
                {
                    dto.ErrorMessage = $"Could not parse date: {datePortion}";
                }
                else
                {
                    dto.Date = easyDate.Value;
                }
            }
        }
    }

    public class HoursInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
        public double Hours { get; set; }
        public bool IsWorkFromHome { get; set; }
        public bool IsBillable { get; set; }
        public string NonBillReason { get; set; }

        public TimeEntryTypeEnum TimeEntryType { get; set; }
    }

    public class ReportInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
    }

    public class DeleteInterpretedCommandDto : CommandDtoBase
    {
    }

    public class CommandDtoBase
    {
        public string ErrorMessage { get; set; }
        
        public DateTime Date { get; set; }
    }
}