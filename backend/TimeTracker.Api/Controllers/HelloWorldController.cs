using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.Api.Controllers
{
    [Route("api/hello")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> SayHello()
        {
            return "Hello from the backend";
        }
    }
}
