using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [ApiController]
    [Route("slack/slashcommand")]
    public class SlackSlashCommandController : ControllerBase
    {
        private readonly SlackMessageOrchestrator messageOrchestrator;

        public SlackSlashCommandController(TimeTrackerDbContext dbContext)
        {
            messageOrchestrator = new SlackMessageOrchestrator(dbContext);
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