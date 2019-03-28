using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Utils;

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
            var splitText = SplitTextToParts(text);
            if (!text.StartsWith("report"))
            {
                return new ReportInterpretedCommandDto()
                    {ErrorMessage = $"Invalid start option: {splitText.FirstOrDefault()}"};
            }

            var dto = new ReportInterpretedCommandDto();
            if (splitText.Count() > 1)
            {
                dto.Project = splitText.ElementAt(1).Text;
            }

            var datePortion = FindDatePart(splitText);

            ProcessDate(datePortion, dto);

            return dto;
        }

        /// <summary>
        /// parse command string and return typed object
        /// </summary>
        /// <param name="text">ex: record au 8 wfh. or record au 8 yesterday, record vacation 8, record sick 6.</param>
        /// <returns></returns>
        public static HoursInterpretedCommandDto InterpretHoursRecordMessage(string text)
        {
            Guard.ThrowIfNull(text);

            var splitText = SplitTextToParts(text);

            Guard.ThrowIfCheckFails(text.StartsWith("record"),
                $"Invalid start option: {splitText.FirstOrDefault()}", nameof(text));

            splitText.First().IsUsed = true;

            var dto = new HoursInterpretedCommandDto();
            var projectOrTypePart = splitText.ElementAt(1);
            projectOrTypePart.IsUsed = true;
            
            dto.IsWorkFromHome = text.Contains("wfh");
            var hoursPart = splitText.FirstOrDefault(x => !x.IsUsed && double.TryParse(x.Text, out var harry));
            if (hoursPart == null)
            {
                dto.ErrorMessage = "No Hours found!";
                return dto;
            }
            dto.Hours = Convert.ToDouble(hoursPart.Text);
            hoursPart.IsUsed = true;

            TimeEntryTypeEnum entryTypeEnum;
            if (Enum.TryParse(projectOrTypePart.Text, true, out entryTypeEnum))
            {
                dto.TimeEntryType = entryTypeEnum;
            }
            else if(projectOrTypePart.Text.Equals("nonbill", StringComparison.OrdinalIgnoreCase))
            {
                dto.IsBillable = false;
                dto.TimeEntryType = TimeEntryTypeEnum.NonBillable;
            }
            else
            {
                dto.TimeEntryType = TimeEntryTypeEnum.BillableProject;
                dto.IsBillable = true;
                dto.Project = projectOrTypePart.Text;
            }
            /* handle date */
            var datePortion = FindDatePart(splitText);
            ProcessDate(datePortion, dto);
            
            /* handle non bill reason */
            if (!dto.IsBillable)
            {
                var startIndexOfReason = text.IndexOf("\"", StringComparison.Ordinal);
                if (startIndexOfReason > 0)
                {
                    dto.NonBillReason = text.Substring(startIndexOfReason).Replace("\"", "").Trim();
                }

                if (string.IsNullOrEmpty(dto.NonBillReason))
                {
                    var possibleNonBills = splitText.Where(x => !x.IsUsed).Select(x => x.Text).ToArray();
                    if (possibleNonBills.Any())
                    {
                        dto.NonBillReason = string.Join(" ", possibleNonBills).Trim();
                    }
                }
            }

            return dto;
        }


        public static DeleteInterpretedCommandDto InterpretDeleteMessage(string text)
        {
            var splitText = SplitTextToParts(text);
            if (!text.StartsWith("delete"))
            {
                return new DeleteInterpretedCommandDto()
                    {ErrorMessage = $"Invalid start option: {splitText.FirstOrDefault()}"};
            }

            var dto = new DeleteInterpretedCommandDto();
            var datePortion = FindDatePart(splitText);

            ProcessDate(datePortion, dto);

            return dto;
        }

        private static List<TextMessagePart> SplitTextToParts(string text)
        {
            return (from t in text.ToLowerInvariant().Split(' ')
                select new TextMessagePart
                {
                    IsUsed = false,
                    Text = t.Trim()
                }).ToList();
        }

        private static void ProcessDate(string datePortion, CommandDtoBase dto)
        {
            if (string.IsNullOrEmpty(datePortion))
            {
                dto.Date = EasyDateParser.GetUtcNow();
            }
            else
            {
                var easyDate = EasyDateParser.ParseEasyDate(datePortion);
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

        /// <summary>
        /// SHould be able to find dates with strings like mar-21, mar-21-2019, march-20, yesterday
        /// </summary>
        /// <param name="splitText"></param>
        /// <returns></returns>
        public static string FindDatePart(List<TextMessagePart> splitText)
        {
            var yesterdayPart = splitText.FirstOrDefault(x => x.Text.Equals("yesterday", StringComparison.OrdinalIgnoreCase));
            if (yesterdayPart != null)
            {
                yesterdayPart.IsUsed = true;
                return "yesterday";
            }

            var foundDatePart = splitText.FirstOrDefault(x => DateTime.TryParseExact(x.Text, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out var parsedDate));
            if (foundDatePart != null)
            {
                foundDatePart.IsUsed = true;
                return foundDatePart.Text;
            }
            string[] monthsShort = {"jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
            var possibleDates = splitText.Where(x => !x.IsUsed
                                                     && monthsShort.Contains(x.Text.Substring(0, x.Text.Length < 3 ? x.Text.Length : 3)) 
                                                     && x.Text.Contains("-"));
            var datePart = possibleDates.FirstOrDefault();
            if (datePart == null)
                return null;
            
            datePart.IsUsed = true;
            
            return datePart.Text;
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