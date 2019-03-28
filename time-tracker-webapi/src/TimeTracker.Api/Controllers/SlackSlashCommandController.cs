using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;

namespace TimeTracker.Api.Controllers
{
    [ApiController]
    [Route("slack/slashcommand")]
    public class SlackSlashCommandController : ControllerBase
    {
        private readonly MessageOrchestrationFactory messageOrchestrationFactory;

        public SlackSlashCommandController(TimeTrackerDbContext dbContext)
        {
            messageOrchestrationFactory = new MessageOrchestrationFactory(dbContext);
        }

        [HttpPost("hours")]
        [Produces("application/json")]
        public async Task<ActionResult<SlackMessage>> HandleCommand([FromForm] SlashCommandPayload slashCommandPayload)
        {
            var orchestration = messageOrchestrationFactory.Create(slashCommandPayload);

            var response = orchestration.GenerateResponse(slashCommandPayload);
            
            return Ok(response);
        }
    }
}