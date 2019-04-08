using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Services.Interpretation;

namespace TimeTracker.Library.Services.Orchestration
{
    public class ReportMessageOrchestration : MessageOrchestration<ReportInterpreter, ReportInterpretedMessage>
    {
        private readonly TimeTrackerDbContext dbContext;

        public ReportMessageOrchestration(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task<SlackMessageResponse> RespondTo(ReportInterpretedMessage message)
        {
            var userService = new UserService(dbContext);
            var user = await userService.FindOrCreateSlackUser(message.UserId, message.UserName);

            var userReportSvc = new UserReportService(dbContext, user.UserId);

            var report = await userReportSvc.GetHoursSummaryMonthAndYtd(null);
            return new SlackMessageResponse(report.ToMessage(), true);
        }
    }
}