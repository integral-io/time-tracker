using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Services.Interpretation;

namespace TimeTracker.Library.Services.Orchestration
{
    public class RecordMessageOrchestration : MessageOrchestration<HoursInterpreter, HoursInterpretedMessage>
    {
        private readonly TimeTrackerDbContext dbContext;

        public RecordMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task<SlackMessageResponse> RespondTo(HoursInterpretedMessage command)
        {
            var userService = new UserService(dbContext);

            var user = await userService.FindOrCreateSlackUser(command.UserId,
                command.UserName);
            var timeEntryService = new TimeEntryService(user.UserId, dbContext);
            
            if (command.IsBillable)
            {
                // resolve client and project
                var projectSvc = new ProjectService(dbContext);
                var project = await projectSvc.FindProjectFromName(command.Project);

                if (project == null)
                {
                    return new SlackMessageResponse($"Invalid Project Name {command.Project}", "error");
                }

                await timeEntryService.CreateBillableTimeEntry(
                    command.Date, command.Hours, 
                    project.BillingClientId, project.ProjectId);
                
                return new SlackMessageResponse(
                    $"Registered *{command.Hours:F1} hours* for project *{command.Project}* {command.Date:D}. " +
                    (command.IsWorkFromHome ? "_Worked From Home_" : ""), "success");
            }

            await timeEntryService.CreateNonBillableTimeEntry(command.Date, command.Hours,
                command.NonBillReason, command.TimeEntryType);
                        
            return new SlackMessageResponse($"Registered *{command.Hours:F1} hours* for Nonbillable reason: {command.NonBillReason ?? command.TimeEntryType.ToString()} for date: {command.Date:D}", "success");
        }
    }
}