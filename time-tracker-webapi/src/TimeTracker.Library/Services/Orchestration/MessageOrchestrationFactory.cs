using System;
using TimeTracker.Data;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Orchestration
{
    public class MessageOrchestrationFactory
    {
        private readonly TimeTrackerDbContext dbContext;

        public MessageOrchestrationFactory(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public MessageOrchestration Create(SlashCommandPayload payload)
        {
            var option = payload.text.GetFirstWord();
            Enum.TryParse(option, true, out SlackMessageOptions optionEnum);
            
            switch (optionEnum)
            {
                case SlackMessageOptions.Record:
                {
                    return new RecordMessageOrchestration(dbContext);
                }
                case SlackMessageOptions.Delete:
                {
                    return new DeleteMessageOrchestration(dbContext);
                }
                case SlackMessageOptions.Help:
                case SlackMessageOptions.Report:
                {
                    return new HelpMessageOrchestration();
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}