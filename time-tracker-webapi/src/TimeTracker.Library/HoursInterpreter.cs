using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Library
{
    public class HoursInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
        public double Hours { get; set; }
        public bool IsWorkFromHome { get; set; }
        public bool IsBillable { get; set; }
        public string NonBillReason { get; set; }

        public TimeEntryTypeEnum TimeEntryType { get; set; }
    }

    public class HoursInterpreter : SlackMessageInterpreter<HoursInterpretedCommandDto>
    {
        public HoursInterpreter() : base("record")
        {
        }

        protected override void ExtractInto(HoursInterpretedCommandDto dto,
            List<TextMessagePart> splitText)
        {
            splitText.First().IsUsed = true;
            var projectOrTypePart = splitText.ElementAt(1);
            projectOrTypePart.IsUsed = true;

            var wfhPart = splitText.FirstOrDefault(x => x.Text == "wfh");
            if (wfhPart != null)
            {
                wfhPart.IsUsed = true;
                dto.IsWorkFromHome = true;
            }

            var hoursPart = splitText.FirstOrDefault(x => !x.IsUsed && double.TryParse(x.Text, out _));
            if (hoursPart == null)
            {
                dto.ErrorMessage = "No Hours found!";
                return;
            }

            dto.Hours = Convert.ToDouble(hoursPart.Text);
            hoursPart.IsUsed = true;

            TimeEntryTypeEnum entryTypeEnum;
            if (Enum.TryParse(projectOrTypePart.Text, true, out entryTypeEnum))
            {
                dto.TimeEntryType = entryTypeEnum;
            }
            else if (projectOrTypePart.Text.Equals("nonbill", StringComparison.OrdinalIgnoreCase))
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

            /* handle non bill reason */
            if (!dto.IsBillable)
            {
                var startIndexOfReason = splitText.FindIndex(x => x.Text.StartsWith("\""));
                if (startIndexOfReason > 0)
                {
                    var stopIndexOfReason = splitText.FindIndex(x => x.Text.EndsWith("\""));
                    var reasonParts = splitText.Skip(startIndexOfReason)
                        .Take(stopIndexOfReason - startIndexOfReason + 1).ToList();

                    dto.NonBillReason = string.Join(" ", reasonParts.Select(x => x.Text))
                        .Replace("\"", "")
                        .Trim();

                    reasonParts.ForEach(x => x.IsUsed = true);
                }
                else
                {
                    var possibleNonBills = splitText.Where(x => !x.IsUsed).ToList();
                    if (possibleNonBills.Any())
                    {
                        dto.NonBillReason = string.Join(" ", possibleNonBills.Select(x => x.Text)).Trim();
                        possibleNonBills.ForEach(x => x.IsUsed = true);
                    }
                }
            }
        }
    }
}