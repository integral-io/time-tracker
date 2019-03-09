using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library.Services
{
    public class SlackMessageOrchestrator
    {
        private readonly TimeTrackerDbContext _dbContext;
        
        public SlackMessageOrchestrator(TimeTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// taks in a slash command, processes it based on the message text, acts on the database and then returns
        /// a response message for the user.
        /// </summary>
        /// <param name="slashCommand"></param>
        /// <returns></returns>
        public async Task<SlackMessage> HandleCommand(SlashCommandPayload slashCommandPayload)
        {
            Guard.ThrowIfNull(slashCommandPayload);
            
            string option = String.IsNullOrWhiteSpace(slashCommandPayload.text) ? "" : slashCommandPayload.text.Split(' ').FirstOrDefault();
            SlackMessageOptions.TryParse(option, true, out SlackMessageOptions optionEnum);
            
            var userSevice = new UserService(_dbContext);
            SlackMessage message;
            
            var user = await userSevice.FindOrCreateSlackUser(slashCommandPayload.user_id, slashCommandPayload.user_name);
            var timeEntryService = new TimeEntryService(user.UserId, _dbContext);
            
            switch (optionEnum)
            {
                case SlackMessageOptions.Record:
                {
                    var commandDto = SlackMessageInterpreter.InterpretHoursRecordMessage(slashCommandPayload.text);
                    
                    if (commandDto.IsBillable)
                    {
                        // resolve client and project
                        var projectSvc = new ProjectService(user.UserId, _dbContext);
                        var project = await projectSvc.FindProjectFromName(commandDto.Project);

                        if (project == null)
                        {
                            // Project was not found
                            message = BuildMessage($"Invalid Project Name {commandDto.Project}", "error");
                            return message;
                        }
                        
                        message = BuildMessage($"Registered *{commandDto.Hours:F1} hours* for project *{commandDto.Project}* {commandDto.Date:D}. " +
                                           (commandDto.IsWorkFromHome ? "_Worked From Home_" : ""), "success");
                        await timeEntryService.CreateBillableTimeEntry(commandDto.Date, commandDto.Hours, project.BillingClientId, project.ProjectId);
                    }
                    else
                    {
                        await timeEntryService.CreateNonBillableTimeEntry(commandDto.Date, commandDto.Hours,
                            commandDto.NonBillReason, commandDto.TimeEntryType);
                        
                        message = BuildMessage($"Registered *{commandDto.Hours:F1} hours* for Nonbillable reason: {commandDto.NonBillReason ?? commandDto.TimeEntryType.ToString()} for date: {commandDto.Date:D}", "success");
                    }

                    return message;
                }
                case SlackMessageOptions.Report:
                {
                    var commandDto = SlackMessageInterpreter.InterpretReportMessage(slashCommandPayload.text);
                    var userReportSvc = new UserReportService(_dbContext, user.UserId);
                    
                    var report = await userReportSvc.GetHoursSummaryMonthAndYtd(null);
                    return BuildMessage(report.ToMessage(), "success");
                }
                case SlackMessageOptions.Delete:
                {
                    var commandDto = SlackMessageInterpreter.InterpretDeleteMessage(slashCommandPayload.text);
                    double hoursDeleted = await timeEntryService.DeleteHours(commandDto.Date);
                    message = BuildMessage($"Deleted {hoursDeleted:F1} hours for date: {commandDto.Date:D}", "success");
                    return message;
                }
                default:
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("*/hours* record <projectName> <hours> _Will use today date by default_");
                    sb.AppendLine("*/hours* record <projectName> <hours> jan-21 _sets date to january 21 current year_");
                    sb.AppendLine("*/hours* record <projectName> <hours> wfh _wfh option marks as worked from home_");
                    sb.AppendLine("*/hours* record nonbill <hours> <optional: date> \"non billable reason\" _non billable hour for a given reason, ie PDA_");
                    sb.AppendLine("*/hours* record sick <hours> <optional: date> _marks sick hours_");
                    sb.AppendLine("*/hours* record vacation <hours> <optional: date> _marks vacation hours_");
                    sb.AppendLine("*/hours* report <optional: date> _generate report of hours_");
                    sb.AppendLine("*/hours* delete <optional: date> _delete all hours for the date_");
                    
                    message = BuildMessage(sb.ToString(), "success");
                    return message;
                }
            }
        }
        
        private SlackMessage BuildMessage(String text, String messageType)
        {
            var message = new SlackMessage()
            {
                Text = messageType == "success" ? text : $"Error: *{text}*"
            };

            return message;
        }
    }
}