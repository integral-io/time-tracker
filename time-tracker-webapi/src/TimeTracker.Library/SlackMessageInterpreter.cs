using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library
{
    public abstract class SlackMessageInterpreter<T> : SlackMessageInterpreter 
        where T : CommandDtoBase, new()
    {
        private readonly string command;

        protected SlackMessageInterpreter(string command)
        {
            this.command = command;
        }

        public T InterpretMessage(SlashCommandPayload payload)
        {
            Guard.ThrowIfNull(payload.text);
            
            var splitText = SplitTextToParts(payload.text);
            
            Guard.ThrowIfCheckFails(payload.text.StartsWith(command),
                $"Invalid start option: {splitText.FirstOrDefault()}", nameof(payload.text));

            var datePortion = FindDatePart(splitText);
            var dto = Create(splitText);
            
            return ProcessDate(datePortion, dto);
        }
        
        protected abstract T Create(List<TextMessagePart> splitText);

        private static List<TextMessagePart> SplitTextToParts(string text)
        {
            return (from t in text.ToLowerInvariant().Split(' ')
                select new TextMessagePart
                {
                    IsUsed = false,
                    Text = t.Trim()
                }).ToList();
        }

        private static T ProcessDate(string datePortion, T dto)
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

            return dto;
        }
    }

    public abstract class SlackMessageInterpreter
    {
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
                return "yesterday";
            }

            var foundDatePart = splitText.FirstOrDefault(x => DateTime.TryParseExact(x.Text, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out _));
            if (foundDatePart != null)
            {
                return foundDatePart.Text;
            }
            string[] monthsShort = {"jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
            var possibleDates = splitText.Where(x => !x.IsUsed
                                                     && monthsShort.Contains(x.Text.Substring(0, x.Text.Length < 3 ? x.Text.Length : 3)) 
                                                     && x.Text.Contains("-"));
            return possibleDates.FirstOrDefault()?.Text;
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