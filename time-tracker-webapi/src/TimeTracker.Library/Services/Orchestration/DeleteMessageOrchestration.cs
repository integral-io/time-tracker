using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Orchestration
{
    public class DeleteMessageOrchestration : MessageOrchestration
    {
        private readonly TimeTrackerDbContext dbContext;

        public DeleteMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task<SlackMessageResponse> RespondTo(SlashCommandPayload slashCommandPayload)
        {
            var commandDto = SlackMessageInterpreter.InterpretDeleteMessage(slashCommandPayload.text);
            var userService = new UserService(dbContext);

            var user = await userService.FindOrCreateSlackUser(slashCommandPayload.user_id,
                slashCommandPayload.user_name);
            
            var timeEntryService = new TimeEntryService(user.UserId, dbContext);
            var hoursDeleted = await timeEntryService.DeleteHours(commandDto.Date);
            
            return new SlackMessageResponse($"Deleted {hoursDeleted:F1} hours for date: {commandDto.Date:D}", "success");
        }
    }
}