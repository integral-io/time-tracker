using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Services.Interpretation;

namespace TimeTracker.Library.Services.Orchestration
{
    public class DeleteMessageOrchestration : MessageOrchestration<DeleteInterpreter, DeleteInterpretedMessage>
    {
        private readonly TimeTrackerDbContext dbContext;

        public DeleteMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task<SlackMessageResponse> RespondTo(DeleteInterpretedMessage command)
        {
            var userService = new UserService(dbContext);

            var user = await userService.FindOrCreateSlackUser(command.UserId,
                command.UserName);
            
            var timeEntryService = new TimeEntryService(user.UserId, dbContext);
            var hoursDeleted = await timeEntryService.DeleteHours(command.Date);
            
            return new SlackMessageResponse($"Deleted {hoursDeleted:F1} hours for date: {command.Date:D}", "success");
        }
    }
}