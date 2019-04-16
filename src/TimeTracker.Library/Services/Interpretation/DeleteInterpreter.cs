using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{
    public class DeleteInterpretedMessage : InterpretedMessage
    {
        public TimeEntryTypeEnum? TimeEntryType { get; set; }
    }

    public class DeleteInterpreter : SlackMessageInterpreter<DeleteInterpretedMessage>
    {
        public DeleteInterpreter() : base("delete")
        {
        }

        public override string HelpMessage => new StringBuilder()
            .AppendLine("*/hours* delete <optional: date> _delete all hours for the date_")
            .AppendLine("*/hours* delete nonbill <optional: date> _delete nonbillable all hours for the date_")
            .AppendLine("*/hours* delete sick <optional: date> _delete all sick hours for the date_")
            .AppendLine("*/hours* delete vacation <optional: date> _delete all vacation hours for the date_")
            .AppendLine("*/hours* delete billable <optional: date> _delete all billable hours for the date_")
            .ToString();

        protected override void ExtractInto(DeleteInterpretedMessage message, List<TextMessagePart> splitText)
        {
            if (splitText.All(x => x.IsUsed))
            {
                return;
            }

            var projectOrTypePart = splitText.First(x => !x.IsUsed);
            projectOrTypePart.IsUsed = true;

            message.TimeEntryType = InterpretTimeEntryType(projectOrTypePart.Text);
        }

        private static TimeEntryTypeEnum? InterpretTimeEntryType(string text)
        {
            if (text == "nonbill")
            {
                return TimeEntryTypeEnum.NonBillable;
            }

            if (text == "billable")
            {
                return TimeEntryTypeEnum.BillableProject;
            }

            if (Enum.TryParse(text, true, out TimeEntryTypeEnum entryTypeEnum))
            {
                return entryTypeEnum;
            }

            return null;
        }
    }
}