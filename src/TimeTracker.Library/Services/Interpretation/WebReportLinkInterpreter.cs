using System;
using System.Collections.Generic;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{
    public class WebReportLinkInterpretedMessage : InterpretedMessage
    {
        public string Url { get; set; }
    }
    
    public class WebReportLinkInterpreter : SlackMessageInterpreter<WebReportLinkInterpretedMessage>
    {
        public WebReportLinkInterpreter() : base("web")
        {
        }

        public override string HelpMessage => "*/hours* web report - _generate a link to a report of hours_";
        protected override void ExtractInto(WebReportLinkInterpretedMessage message, List<TextMessagePart> splitText)
        {
            if (splitText[1].Text == "report")
            {
                message.Url = "http://integral.io";
            }
            else
            {
                message.Url = "http://google.com";
            }
        }
    }
}