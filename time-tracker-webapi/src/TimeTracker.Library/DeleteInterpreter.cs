using System.Collections.Generic;
using TimeTracker.Library.Models;

namespace TimeTracker.Library
{
    public class DeleteInterpreter : SlackMessageInterpreter<DeleteInterpretedCommandDto>
    {
        public DeleteInterpreter() : base("delete")
        {
        }

        protected override DeleteInterpretedCommandDto Create(List<TextMessagePart> splitText)
        {
            return new DeleteInterpretedCommandDto();
        }
    }
}