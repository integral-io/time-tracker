using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Services.Interpretation;

namespace TimeTracker.Library.Services.Orchestration
{
    public class WebMessageOrchestration : MessageOrchestration<WebReportLinkInterpreter, WebReportLinkInterpretedMessage>
    {
        private readonly TimeTrackerDbContext dbContext;

        public WebMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        protected override Task<SlackMessageResponse> RespondTo(WebReportLinkInterpretedMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}