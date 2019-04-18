using System.Collections.Generic;
using System.Text;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{

    public class ReportInterpretedMessage : InterpretedMessage
    {
    }
    
    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedMessage>
    {
        public ReportInterpreter() : base("report")
        {
        }

        public override string HelpMessage => new StringBuilder()
            .AppendLine("*/hours* report <optional: date> _generate report of hours for monthly and ytd_")
            .AppendLine("*/hours* report month <month> _generate report of hours for month_")
            .AppendLine("*/hours* report year <year> _generate report of hours for year_")
            .ToString();

        protected override void ExtractInto(ReportInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
            if (splitText.Count > 1)
            {
                //SlackMessageInterpreter.FindDatePart(splitText.ElementAt(1));
                //todo message.Date = splitText.ElementAt(1).Text;
            }
        }
    }
}