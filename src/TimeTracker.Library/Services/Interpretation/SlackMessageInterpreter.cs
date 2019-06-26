using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library.Services.Interpretation
{
    public abstract class SlackMessageInterpreter<T> : SlackMessageInterpreter 
        where T : InterpretedMessage, new()
    {
        private readonly SlackMessageOptions option;
        public abstract string HelpMessage { get; }

        protected SlackMessageInterpreter(SlackMessageOptions option)
        {
            this.option = option;
        }

        public virtual T InterpretMessage(SlashCommandPayload payload)
        {
            Guard.ThrowIfNull(payload.text);

            var splitText = SplitTextToParts(payload.text);

            Guard.ThrowIfCheckFails(payload.text.StartsWith(option.ToString().ToLower()),
                $"Invalid start option: {splitText.FirstOrDefault()}", nameof(payload.text));
            
            splitText.First().IsUsed = true;            

            if (IsHelpRequest(splitText.Where(x => !x.IsUsed)))
            {
                return new T
                {
                    IsHelp = true
                };
            }

            var message = new T
            {
                Date = ExtractDate(splitText),
                UserId = payload.user_id,
                UserName = payload.user_name
            };

            ExtractInto(message, splitText);

            if (splitText.Any(x => !x.IsUsed))
                message.ErrorMessage = $"Not sure how to interpret '{splitText.Where(x => !x.IsUsed).Select(x => x.Text).Join(" ")}'";

            return message;
        }

        private static bool IsHelpRequest(IEnumerable<TextMessagePart> splitText)
        {
            var helpPart = splitText.FirstOrDefault();
            
            return helpPart?.Text == "help";
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
        
        protected abstract void ExtractInto(T message, List<TextMessagePart> splitText);

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

    public abstract class InterpretedMessage
    {
        public string ErrorMessage { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool IsHelp { get; set; }

        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}