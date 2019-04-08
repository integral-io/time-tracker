using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{
    public class HoursInterpretedMessage : InterpretedMessage
    {
        public string Project { get; set; }
        public double Hours { get; set; }
        public bool IsWorkFromHome { get; set; }
        public bool IsBillable { get; set; }
        public string NonBillReason { get; set; }

        public TimeEntryTypeEnum TimeEntryType { get; set; }
    }

    public class HoursInterpreter : SlackMessageInterpreter<HoursInterpretedMessage>
    {
        public HoursInterpreter() : base("record")
        {
        }

        public override string HelpMessage => new StringBuilder()
            .AppendLine("*/hours* record <projectName> <hours> _Will use today date by default_")
            .AppendLine("*/hours* record <projectName> <hours> jan-21 _sets date to january 21 current year_")
            .AppendLine("*/hours* record <projectName> <hours> wfh _wfh option marks as worked from home_")
            .AppendLine("*/hours* record nonbill <hours> <optional: date> \"non billable reason\" _non billable hour for a given reason, ie PDA_")
            .AppendLine("*/hours* record sick <hours> <optional: date> _marks sick hours_")
            .AppendLine("*/hours* record vacation <hours> <optional: date> _marks vacation hours_")
            .ToString();

        protected override void ExtractInto(HoursInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
            var hours = InterpretHours(splitText.Where(x => !x.IsUsed));
            if (hours.HasValue)
                message.Hours = hours.Value;
            else
            {
                message.ErrorMessage = "No Hours found!";
                return;
            }

            var projectOrTypePart = splitText.First(x => !x.IsUsed);
            projectOrTypePart.IsUsed = true;
         
            var interpretTimeEntryType = InterpretTimeEntryType(projectOrTypePart.Text);
            if (interpretTimeEntryType.HasValue)
            {
                message.TimeEntryType = interpretTimeEntryType.Value;
            }
            else
            {
                message.TimeEntryType = TimeEntryTypeEnum.BillableProject;
                message.IsBillable = true;
                message.Project = projectOrTypePart.Text;
            }

            message.IsWorkFromHome = InterpretIsWorkingFromHome(splitText.Where(x => !x.IsUsed));

            if (!message.IsBillable)
            {
                message.NonBillReason = InterpretNonBillableReason(splitText.Where(x => !x.IsUsed).ToList());
            }
        }

        private static double? InterpretHours(IEnumerable<TextMessagePart> unusedParts)
        {
            double hours = 0;
            var hoursPart = unusedParts.FirstOrDefault(x => double.TryParse(x.Text, out hours));
            if (hoursPart == null)
                return null;

            hoursPart.IsUsed = true;
            return hours;
        }

        private static TimeEntryTypeEnum? InterpretTimeEntryType(string text)
        {
            if (text == "nonbill")
            {
                return TimeEntryTypeEnum.NonBillable;
            }

            if (Enum.TryParse(text, true, out TimeEntryTypeEnum entryTypeEnum))
            {
                return entryTypeEnum;
            }

            return null;
        }

        private static bool InterpretIsWorkingFromHome(IEnumerable<TextMessagePart> unusedParts)
        {
            var wfhPart = unusedParts.FirstOrDefault(x => x.Text == "wfh");
            if (wfhPart == null) 
                return false;

            wfhPart.IsUsed = true;
            return true;
        }

        private static string InterpretNonBillableReason(List<TextMessagePart> unusedParts)
        {
            unusedParts.ForEach(x => x.IsUsed = true);

            return string.Join(" ", unusedParts.Select(x => x.Text))
                .Replace("\"", "")
                .Trim();
        }
    }
}