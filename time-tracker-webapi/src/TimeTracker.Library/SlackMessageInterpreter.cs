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
            splitText.First().IsUsed = true;

            var dto = new T
            {
                Date = ExtractDate(splitText)
            };

            ExtractInto(dto, splitText);
            
            if (splitText.Any(x => !x.IsUsed))
                throw new Exception("Not fully sure what to do here");

            return dto;
        }

        private static DateTime ExtractDate(IEnumerable<TextMessagePart> splitText)
        {
            var datePortion = FindDatePart(splitText);
            if (datePortion == null)
            {
                return EasyDateParser.GetUtcNow();
            }

            datePortion.IsUsed = true;
            return EasyDateParser.ParseEasyDate(datePortion.Text);
        }

        protected abstract void ExtractInto(T dto, List<TextMessagePart> splitText);

        private static List<TextMessagePart> SplitTextToParts(string text)
        {
            return (from t in text.ToLowerInvariant().Split(' ')
                where !string.IsNullOrWhiteSpace(t)
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
            return splitText.FirstOrDefault(x => !x.IsUsed && EasyDateParser.IsSupportedDate(x.Text));            
        }
    }

    public abstract class CommandDtoBase
    {
        public string ErrorMessage { get; set; }

        public DateTime Date { get; set; }
    }
}