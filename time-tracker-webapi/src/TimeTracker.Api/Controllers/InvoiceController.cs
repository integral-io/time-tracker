using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;
using TimeTracker.Data;

namespace TimeTracker.Api.Controllers
{
    [Route("invoice")]
    public class InvoiceController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;

        public InvoiceController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        [HttpGet("month/{monthDate}"), Authorize]
        public async Task<IActionResult> Month(string monthDate)
        {
            return View(new InvoiceDto());
        }
        
        
    }
}