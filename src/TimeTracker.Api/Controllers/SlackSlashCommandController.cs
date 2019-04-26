using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;

namespace TimeTracker.Api.Controllers
{
    [ApiController]
    [Route("slack/slashcommand")]
    public class SlackSlashCommandController : ControllerBase
    {
        private readonly SlackMessageOrchestrator messageOrchestrator;
        private const string WebAppUriKey = "WebAppUri";

        public SlackSlashCommandController(TimeTrackerDbContext dbContext, IConfiguration config)
        {
            messageOrchestrator = new SlackMessageOrchestrator(dbContext, config.GetValue<String>(WebAppUriKey));
        }

        [HttpPost("hours")]
        [Produces("application/json")]
        public async Task<ActionResult<SlackMessage>> HandleCommand([FromForm] SlashCommandPayload slashCommandPayload)
        {
            var message = await messageOrchestrator.HandleCommand(slashCommandPayload);
            
            return Ok(message);
        }
    }
}