using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;
using TimeTracker.Api.Services;
using TimeTracker.Data;

namespace TimeTracker.Api.Controllers
{
    [ApiController]
    [Route("slack/slashcommand")]
    public class SlackSlashCommandController : ControllerBase
    {
        private readonly TimeTrackerDbContext _dbContext;

        public SlackSlashCommandController(TimeTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("hours")]
        [Produces("application/json")]
        public async Task<ActionResult<SlackMessage>> HandleCommand([FromForm]SlashCommandPayload slashCommandPayload)
        {
            string option = slashCommandPayload.text.Split(' ').FirstOrDefault();
            var userSevice = new UserService(_dbContext);
            SlackMessage message;
            
            var user = await userSevice.FindOrCreateSlackUser(slashCommandPayload.user_id, slashCommandPayload.user_name);
            var timeEntryService = new TimeEntryService(user.UserId, _dbContext);
            
            switch (option)
            {
                case SlackMessageInterpreter.OPTION_RECORD:
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
                            return Ok(message);
                        }
                        
                        await timeEntryService.CreateBillableTimeEntry(commandDto.Date, commandDto.Hours, project.BillingClientId, project.ProjectId);
                    }

                    message = BuildMessage($"Registered *{commandDto.Hours:F1} hours* for project *{commandDto.Project}* {commandDto.Date:D}. " +
                                           (commandDto.IsWorkFromHome ? "_Worked From Home_" : ""), "success");
                    return Ok(message);
                }
                case SlackMessageInterpreter.OPTION_REPORT:
                {
                    var commandDto = SlackMessageInterpreter.InterpretReportMessage(slashCommandPayload.text);
                    
                    // todo: query billable hours for project
                    var report = await timeEntryService.QueryHours(commandDto.Project, commandDto.StartDateMonth);
                    
                    
                    break;
                }
            }

            return Ok("unsupported option");
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