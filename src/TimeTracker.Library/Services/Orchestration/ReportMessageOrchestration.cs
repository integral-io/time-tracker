using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Models;
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

            TimeEntryReport report;
            if (message.Month != null)
            {
                report = await userReportSvc.GetHoursSummaryMonth(message.Date.Month);
                return new SlackMessageResponse(report.ToMonthlyMessage(), true);
            }
            else if (message.ReportYear)
            {
                report = await userReportSvc.GetHoursSummaryYear(message.Date.Year);
                return new SlackMessageResponse(report.ToYearlyMessage(), true);
            }
            else
            {
                report = await userReportSvc.GetHoursSummaryMonthAndYtd(null);
                return new SlackMessageResponse(report.ToMonthAndYTDMessage(), true);
            }
        }
    }
}