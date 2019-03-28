using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Orchestration
{
    public class ReportMessageOrchestration : MessageOrchestration
    {
        private readonly TimeTrackerDbContext dbContext;

        public ReportMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task<SlackMessageResponse> RespondTo(SlashCommandPayload slashCommandPayload)
        {
            SlackMessageInterpreter.InterpretReportMessage(slashCommandPayload.text);
            
            var userService = new UserService(dbContext);
            var user = await userService.FindOrCreateSlackUser(slashCommandPayload.user_id, slashCommandPayload.user_name);
            
            var userReportSvc = new UserReportService(dbContext, user.UserId);
                    
            var report = await userReportSvc.GetHoursSummaryMonthAndYtd(null);
            return new SlackMessageResponse(report.ToMessage(), "success");
        }
    }
}