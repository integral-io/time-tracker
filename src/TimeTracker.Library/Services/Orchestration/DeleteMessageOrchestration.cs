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

        protected override async Task<SlackMessageResponse> RespondTo(DeleteInterpretedMessage message)
        {
            var userService = new UserService(dbContext);

            var user = await userService.FindOrCreateSlackUser(message.UserId, message.UserName);

            var timeEntryService = new TimeEntryService(user.UserId, dbContext);

            double hoursDeleted;
            if (message.TimeEntryType.HasValue)
            {
                hoursDeleted = await timeEntryService.DeleteHoursForTimeEntryType(message.Date, message.TimeEntryType.Value);
                return new SlackMessageResponse($"Deleted {hoursDeleted:F1} {message.TimeEntryType} hours for date: {message.Date:D}", true);

            }

            hoursDeleted = await timeEntryService.DeleteHours(message.Date);
            return new SlackMessageResponse($"Deleted {hoursDeleted:F1} hours for date: {message.Date:D}", true);
        }
    }
}