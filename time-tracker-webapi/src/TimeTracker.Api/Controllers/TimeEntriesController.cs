using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TimeTracker.Api.Models;
using TimeTracker.Library.Models;


namespace TimeTracker.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/timerecords")]
    public class TimeEntriesController : ControllerBase
    {
        [HttpPost("push"), ProducesResponseType(typeof(IEnumerable<Guid>), 201), ProducesResponseType(typeof(ModelStateDictionary),400)]
        public async Task<ActionResult<IEnumerable<Guid>>> PushRecords(ICollection<TimeEntry> entries)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // determine user from token
            // persist data
            
            // returns the created Id's
            return Ok();
        }
    }
}