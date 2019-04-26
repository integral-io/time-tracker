using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Services.Interpretation;

namespace TimeTracker.Library.Services.Orchestration
{
    public class WebMessageOrchestration : MessageOrchestration<WebLinkInterpreter, WebLinkInterpretedMessage>
    {
        private readonly TimeTrackerDbContext dbContext;
        private readonly string webAppUri;

        public WebMessageOrchestration(TimeTrackerDbContext dbContext, string webAppUri)
        {
            this.dbContext = dbContext;
            this.webAppUri = webAppUri;
        }
        protected override async Task<SlackMessageResponse> RespondTo(WebLinkInterpretedMessage message)
        {
            var link = $"{webAppUri}/account/linkslack?slackuser={message.UserId}";
            return new SlackMessageResponse("Click this link " + link + " to access your hours on the web.", true);
        }
    }
}