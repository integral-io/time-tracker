using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Data.Models;
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

        protected override async Task<SlackMessageResponse> RespondTo(HoursInterpretedMessage message)
        {
            var userService = new UserService(dbContext);

            var user = await userService.FindOrCreateSlackUser(message.UserId,message.UserName);
            var timeEntryService = new TimeEntryService(user.UserId, dbContext);

            if (message.IsBillable)
            {
                // resolve client and project
                var projectSvc = new ProjectService(dbContext);
                var project = await projectSvc.FindProjectFromName(message.Project);

                if (project == null)
                {
                    return new SlackMessageResponse($"Invalid Project Name {message.Project}", false);
                }

                await timeEntryService.CreateBillableTimeEntry(
                    message.Date, message.Hours, project.ProjectId);

                return new SlackMessageResponse(
                    $"Registered *{message.Hours:F1} hours* for project *{message.Project}* {message.Date:D}. " +
                    (message.IsWorkFromHome ? "_Worked From Home_" : ""), true);
            }
            
            switch (message.TimeEntryType)
            {
                case TimeEntryTypeEnum.Sick:
                    await timeEntryService.CreateNonBillableTimeEntry(message.Date, message.Hours,
                        message.NonBillReason, message.TimeEntryType);
                
                    return new SlackMessageResponse(
                        $"Registered *{message.Hours:F1} hours* for Sick reason: {message.NonBillReason} for date: {message.Date:D}",
                        true);
                
                case TimeEntryTypeEnum.Vacation:
                    await timeEntryService.CreateNonBillableTimeEntry(message.Date, message.Hours,
                        message.NonBillReason, message.TimeEntryType);
                
                    return new SlackMessageResponse(
                        $"Registered *{message.Hours:F1} hours* for Vacation for date: {message.Date:D}",
                        true);
                default:
                    await timeEntryService.CreateNonBillableTimeEntry(message.Date, message.Hours,
                        message.NonBillReason, message.TimeEntryType);

                    return new SlackMessageResponse(
                        $"Registered *{message.Hours:F1} hours* for Nonbillable reason: {message.NonBillReason ?? message.TimeEntryType.ToString()} for date: {message.Date:D}",
                        true);
            }
        }
    }
}