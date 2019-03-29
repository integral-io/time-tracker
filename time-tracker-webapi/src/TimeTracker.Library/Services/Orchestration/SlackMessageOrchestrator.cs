using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Orchestration
{
    public class SlackMessageOrchestrator
    {
        private readonly MessageOrchestrationFactory messageOrchestrationFactory;

        public SlackMessageOrchestrator(TimeTrackerDbContext dbContext)
        {
            messageOrchestrationFactory = new MessageOrchestrationFactory(dbContext);
        }

        /// <summary>
        /// Takes in a slash command, processes it based on the message text, acts on the database and then returns
        /// a response message for the user.
        /// </summary>
        /// <param name="slashCommandPayload"></param>
        /// <returns></returns>
        public Task<SlackMessage> HandleCommand(SlashCommandPayload slashCommandPayload)
        {
            var orchestration = messageOrchestrationFactory.Create(slashCommandPayload);
            
            return orchestration.GenerateResponse(slashCommandPayload);
        }
    }
}