using System.Collections.Generic;
using System.Linq;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;

namespace TimeTracker.Library.Services.Interpretation
{

    public class ProjectsInterpretedMessage : InterpretedMessage
    {
        public string Projects { get; set; }
    }
    public class ProjectsInterpreter : SlackMessageInterpreter<ProjectsInterpretedMessage>
    {
        public override string HelpMessage => "*/hours* projects _display a list of available projects_";

        public ProjectsInterpreter() : base(SlackMessageOptions.Projects)
        {
        }
        protected override void ExtractInto(ProjectsInterpretedMessage message, List<TextMessagePart> splitText)
        {
            if (splitText.Count > 1)
            {
                message.Projects = splitText.ElementAt(1).Text;
            }
        }
    }
}