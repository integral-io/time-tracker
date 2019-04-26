using System;
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

            return await DecideWhichReportToGenerate(message, userReportSvc);
        }

        private static async Task<SlackMessageResponse> DecideWhichReportToGenerate(ReportInterpretedMessage message, UserReportService userReportSvc)
        {
            switch (message.ToGenerate)
            {
                case ReportType.ForSpecificDate:
                    return await GenerateDateReport(message, userReportSvc);
                case ReportType.ForMonth:
                    return await GenerateMonthReport(message, userReportSvc);
                case ReportType.ForYear:
                    return await GenerateYearReport(message, userReportSvc);
                case ReportType.ForMostRecent:
                    return await GenerateLast10EntryReport(userReportSvc);
                default:
                    return await GenerateDefaultReport(userReportSvc);
            }
        }

        private static async Task<SlackMessageResponse> GenerateLast10EntryReport(UserReportService userReportSvc)
        {
            var stringReport = await userReportSvc.GetLastTenEntries();
            return new SlackMessageResponse(stringReport, true);
        }

        private static async Task<SlackMessageResponse> GenerateDefaultReport(UserReportService userReportSvc)
        {
            TimeEntryReport report = await userReportSvc.GetHoursSummaryDefaultWeekMonthAndYtd();
            return new SlackMessageResponse(report.ToWeekMonthAndYTDMessage(), true);
        }

        private static async Task<SlackMessageResponse> GenerateYearReport(ReportInterpretedMessage message, UserReportService userReportSvc)
        {
            TimeEntryReport report = await userReportSvc.GetHoursSummaryYear(message.Date.Year);
            return new SlackMessageResponse(report.ToYearlyMessage(), true);
        }

        private static async Task<SlackMessageResponse> GenerateMonthReport(ReportInterpretedMessage message, UserReportService userReportSvc)
        {
            TimeEntryReport report = await userReportSvc.GetHoursSummaryMonth(message.Date.Month, Convert.ToInt32(message.Year));
            return new SlackMessageResponse(report.ToMonthlyMessage(), true);
        }

        private static async Task<SlackMessageResponse> GenerateDateReport(ReportInterpretedMessage message, UserReportService userReportSvc)
        {
            TimeEntryReport report = await userReportSvc.GetHoursSummaryForDay(message.Date);
            return new SlackMessageResponse(report.ToDayMessage(), true);
        }

    }
} 