using System.Collections.Generic;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;

namespace TimeTracker.Library.Services.Interpretation
{
    public class WebLinkInterpretedMessage : InterpretedMessage
    {
    }

    public class WebLinkInterpreter : SlackMessageInterpreter<WebLinkInterpretedMessage>
    {
        public WebLinkInterpreter() : base(SlackMessageOptions.Web)
        {
        }

        public override string HelpMessage => "*/hours* web - _generate a link to a report of hours_";

        protected override void ExtractInto(WebLinkInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
        }
    }
}