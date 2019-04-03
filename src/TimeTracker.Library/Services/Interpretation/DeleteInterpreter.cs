using System.Collections.Generic;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{

    public class DeleteInterpretedCommandDto : CommandDtoBase
    {
    }
    
    public class DeleteInterpreter : SlackMessageInterpreter<DeleteInterpretedCommandDto>
    {
        public DeleteInterpreter() : base("delete")
        {
        }

        protected override void ExtractInto(DeleteInterpretedCommandDto dto,
            List<TextMessagePart> splitText)
        {
        }
    }
}