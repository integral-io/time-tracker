using System.Collections.Generic;
using System.Linq;
using TimeTracker.Library.Models;

namespace TimeTracker.Library
{
    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedCommandDto>
    {
        public ReportInterpreter() : base("report")
        {
        }

        protected override ReportInterpretedCommandDto Create(List<TextMessagePart> splitText)
        {
            var dto = new ReportInterpretedCommandDto();
            
            if (splitText.Count > 1)
            {
                dto.Project = splitText.ElementAt(1).Text;
            }

            return dto;
        }
    }
}