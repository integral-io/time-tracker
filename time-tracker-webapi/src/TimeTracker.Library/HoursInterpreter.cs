using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Library
{
    public class HoursInterpreter : SlackMessageInterpreter<HoursInterpretedCommandDto>
    {
        public HoursInterpreter() : base("record")
        {
        }

        protected override HoursInterpretedCommandDto Create(List<TextMessagePart> splitText)
        {
            splitText.First().IsUsed = true;

            var dto = new HoursInterpretedCommandDto();
            var projectOrTypePart = splitText.ElementAt(1);
            projectOrTypePart.IsUsed = true;

            dto.IsWorkFromHome = splitText.Any(x => x.Text == "wfh");
            var hoursPart = splitText.FirstOrDefault(x => !x.IsUsed && double.TryParse(x.Text, out _));
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
                var text = string.Join(" ", splitText.Select(x => x.Text));
                
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
    }
}