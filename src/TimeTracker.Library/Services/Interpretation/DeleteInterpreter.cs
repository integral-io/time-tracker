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

        protected override void ExtractInto(DeleteInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
        }
    }
}