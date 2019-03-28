using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Orchestration
{
    public class RecordMessageOrchestration : MessageOrchestration
    {
        private readonly TimeTrackerDbContext dbContext;

        public RecordMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task<SlackMessageResponse> RespondTo(SlashCommandPayload slashCommandPayload)
        {
            var commandDto = SlackMessageInterpreter.InterpretHoursRecordMessage(slashCommandPayload.text);
            
            var userService = new UserService(dbContext);

            var user = await userService.FindOrCreateSlackUser(slashCommandPayload.user_id,
                slashCommandPayload.user_name);
            var timeEntryService = new TimeEntryService(user.UserId, dbContext);
            
            if (commandDto.IsBillable)
            {
                // resolve client and project
                var projectSvc = new ProjectService(dbContext);
                var project = await projectSvc.FindProjectFromName(commandDto.Project);

                if (project == null)
                {
                    return new SlackMessageResponse($"Invalid Project Name {commandDto.Project}", "error");
                }

                await timeEntryService.CreateBillableTimeEntry(
                    commandDto.Date, commandDto.Hours, 
                    project.BillingClientId, project.ProjectId);
                
                return new SlackMessageResponse(
                    $"Registered *{commandDto.Hours:F1} hours* for project *{commandDto.Project}* {commandDto.Date:D}. " +
                    (commandDto.IsWorkFromHome ? "_Worked From Home_" : ""), "success");
            }

            await timeEntryService.CreateNonBillableTimeEntry(commandDto.Date, commandDto.Hours,
                commandDto.NonBillReason, commandDto.TimeEntryType);
                        
            return new SlackMessageResponse($"Registered *{commandDto.Hours:F1} hours* for Nonbillable reason: {commandDto.NonBillReason ?? commandDto.TimeEntryType.ToString()} for date: {commandDto.Date:D}", "success");
        }
    }
}