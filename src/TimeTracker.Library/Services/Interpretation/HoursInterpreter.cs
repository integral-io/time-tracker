using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override void ExtractInto(HoursInterpretedMessage dto,
            List<TextMessagePart> splitText)
        {
            var hours = InterpretHours(splitText.Where(x => !x.IsUsed));
            if (hours.HasValue)
                dto.Hours = hours.Value;
            else
            {
                dto.ErrorMessage = "No Hours found!";
                return;
            }

            var projectOrTypePart = splitText.First(x => !x.IsUsed);
            projectOrTypePart.IsUsed = true;
         
            var interpretTimeEntryType = InterpretTimeEntryType(projectOrTypePart.Text);
            if (interpretTimeEntryType.HasValue)
            {
                dto.TimeEntryType = interpretTimeEntryType.Value;
            }
            else
            {
                dto.TimeEntryType = TimeEntryTypeEnum.BillableProject;
                dto.IsBillable = true;
                dto.Project = projectOrTypePart.Text;
            }

            dto.IsWorkFromHome = InterpretIsWorkingFromHome(splitText.Where(x => !x.IsUsed));

            if (!dto.IsBillable)
            {
                dto.NonBillReason = InterpretNonBillableReason(splitText.Where(x => !x.IsUsed).ToList());
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