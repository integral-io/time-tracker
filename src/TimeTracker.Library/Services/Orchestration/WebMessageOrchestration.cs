using System.Linq;
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
        protected override async Task<SlackMessageResponse> RespondTo(WebReportLinkInterpretedMessage message)
        {
            var link = "https://localhost:5001/user/web/" + message.UserId;
            return new SlackMessageResponse("Click this link " + link + " to access your hours on the web.", true);
        }
    }
}