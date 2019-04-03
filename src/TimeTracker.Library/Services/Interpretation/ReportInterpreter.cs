using System.Collections.Generic;
using System.Linq;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{

    public class ReportInterpretedCommandDto : CommandDtoBase
    {
        public string Project { get; set; }
    }
    
    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedCommandDto>
    {
        public ReportInterpreter() : base("report")
        {
        }

        protected override void ExtractInto(ReportInterpretedCommandDto dto,
            List<TextMessagePart> splitText)
        {
            if (splitText.Count > 1)
            {
                dto.Project = splitText.ElementAt(1).Text;
            }
        }
    }
}