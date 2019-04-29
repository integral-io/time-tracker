using System;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library.Services.Orchestration
{
    public class MessageOrchestrationFactory
    {
        private readonly TimeTrackerDbContext dbContext;
        private readonly string webAppUri;

        public MessageOrchestrationFactory(TimeTrackerDbContext dbContext, string webAppUri)
        {
            this.dbContext = dbContext;
            this.webAppUri = webAppUri;
        }

        public IMessageOrchestration Create(SlashCommandPayload payload) 
        {
            var option = payload.text.GetFirstWord();
            Enum.TryParse(option, true, out SlackMessageOptions optionEnum);
            
            switch (optionEnum)
            {
                case SlackMessageOptions.Web:
                    return new WebMessageOrchestration(dbContext, webAppUri);
                case SlackMessageOptions.Record:
                    return new RecordMessageOrchestration(dbContext);
                case SlackMessageOptions.Delete:
                    return new DeleteMessageOrchestration(dbContext);
                case SlackMessageOptions.Summary:
                    return new ReportMessageOrchestration(dbContext);
                case SlackMessageOptions.Projects:
                        return new ProjectsMessageOrchestration(dbContext);
                case SlackMessageOptions.Help:
                    return new HelpMessageOrchestration();
                default:
                    return new HelpMessageOrchestration();
            }
        }
    }
}