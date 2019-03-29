using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            var dto = new T();
            if (datePortion != null)
            {
                datePortion.IsUsed = true;
                var easyDate = EasyDateParser.ParseEasyDate(datePortion.Text);
                if (!easyDate.HasValue)
                {
                    dto.ErrorMessage = $"Could not parse date: {datePortion.Text}";
                }
                else
                {
                    dto.Date = easyDate.Value;
                }
            }

            ExtractInto(dto, splitText);

            if (string.IsNullOrEmpty(datePortion?.Text))
            {
                dto.Date = EasyDateParser.GetUtcNow();
            }

            return dto;
        }
        
        protected abstract void ExtractInto(T dto, List<TextMessagePart> splitText);

        private static List<TextMessagePart> SplitTextToParts(string text)
        {
            return (from t in text.ToLowerInvariant().Split(' ')
                select new TextMessagePart
                {
                    IsUsed = false,
                    Text = t.Trim()
                }).ToList();
        }
    }

    public abstract class SlackMessageInterpreter
    {
        /// <summary>
        /// SHould be able to find dates with strings like mar-21, mar-21-2019, march-20, yesterday
        /// </summary>
        /// <param name="splitText"></param>
        /// <returns></returns>
        public static TextMessagePart FindDatePart(IEnumerable<TextMessagePart> splitText)
        {
            var supportedValues = new List<Func<string, bool>>
            {
                x => x.Equals("yesterday", StringComparison.OrdinalIgnoreCase),
                IsSupportedDateFormat
            };

            return splitText.FirstOrDefault(x => !x.IsUsed && supportedValues.Any(y => y(x.Text)));            
        }

        private static bool IsSupportedDateFormat(string text)
        {
            var supportedDateFormats = new List<string>
            {
                "yyyy-MM-dd",
                "MMM-dd",
                "MMM-dd-yyyy"
            };

            return supportedDateFormats.Any(x =>
                DateTime.TryParseExact(text, x, new CultureInfo("en-US"), DateTimeStyles.None, out _));
        }
    }

    public abstract class CommandDtoBase
    {
        public string ErrorMessage { get; set; }

        public DateTime Date { get; set; }
    }
}