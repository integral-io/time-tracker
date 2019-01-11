using System;
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
        public async Task<ActionResult<String>> HandleCommand([FromForm]SlashCommandPayload slashCommandPayload)
        {
            return Ok("Hours registered");
        }
    }
}