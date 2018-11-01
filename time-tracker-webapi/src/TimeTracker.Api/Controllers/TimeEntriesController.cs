using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;


namespace TimeTracker.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/timerecords")]
    public class TimeEntriesController : ControllerBase
    {
        [HttpPost("push")]
        public async Task<IActionResult> PushRecords(ICollection<TimeEntryDto> entries)
        {
            // determine user from token
            // persist data
            
            return Ok();
        }
    }
}