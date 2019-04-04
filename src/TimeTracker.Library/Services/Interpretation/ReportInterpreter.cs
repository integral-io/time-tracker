using System.Collections.Generic;
using System.Linq;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{

    public class ReportInterpretedMessage : InterpretedMessage
    {
        public string Project { get; set; }
    }
    
    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedMessage>
    {
        public ReportInterpreter() : base("report")
        {
        }

        protected override void ExtractInto(ReportInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
            if (splitText.Count > 1)
            {
                message.Project = splitText.ElementAt(1).Text;
            }
        }
    }
}