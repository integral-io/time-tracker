using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;

namespace TimeTracker.Api.Controllers
{
    [ApiController]
    [Route("slack/slashcommand")]
    public class SlackSlashCommandController : ControllerBase
    {
        [HttpPost("hours")]
        [Produces("application/json")]
        public async Task<ActionResult<SlackMessage>> HandleCommand([FromForm]SlashCommandPayload slashCommandPayload)
        {
            string option = slashCommandPayload.text.Split(' ').FirstOrDefault();
            
            switch (option)
            {
                case SlackMessageInterpreter.OPTION_RECORD:
                {
                    var commandDto = SlackMessageInterpreter.InterpretHoursRecordMessage(slashCommandPayload.text);
                    var message = new SlackMessage()
                    {
                        Text = $"Registered *{commandDto.Hours:F1} hours* for project *{commandDto.Project}* today. " + 
                               (commandDto.IsWorkFromHome ? "_Worked From Home_" : "")
                    };
                    return Ok(message);
                    break;
                }
                case SlackMessageInterpreter.OPTION_REPORT:
                {
                    break;
                }
            }

            return Ok("unsupported option");
        }
    }
}