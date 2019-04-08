using System.Collections.Generic;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{

    public class DeleteInterpretedMessage : InterpretedMessage
    {
    }
    
    public class DeleteInterpreter : SlackMessageInterpreter<DeleteInterpretedMessage>
    {
        public DeleteInterpreter() : base("delete")
        {
        }

        public override string HelpMessage => "*/hours* delete <optional: date> _delete all hours for the date_";

        protected override void ExtractInto(DeleteInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
        }
    }
}