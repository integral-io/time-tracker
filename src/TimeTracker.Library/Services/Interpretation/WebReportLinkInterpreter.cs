using System.Collections.Generic;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;

namespace TimeTracker.Library.Services.Interpretation
{
    public class WebReportLinkInterpretedMessage : InterpretedMessage
    {
    }

    public class WebReportLinkInterpreter : SlackMessageInterpreter<WebReportLinkInterpretedMessage>
    {
        public WebReportLinkInterpreter() : base(SlackMessageOptions.Web)
        {
        }

        public override string HelpMessage => "*/hours* web - _generate a link to a report of hours_";

        protected override void ExtractInto(WebReportLinkInterpretedMessage message, List<TextMessagePart> splitText)
        {
        }
    }
}