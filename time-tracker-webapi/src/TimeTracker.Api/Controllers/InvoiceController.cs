using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;
using TimeTracker.Data;
using TimeTracker.Library.Models;

namespace TimeTracker.Api.Controllers
{
    [Route("invoice")]
    public class InvoiceController : Controller
    {
        private readonly TimeTrackerDbContext _dbContext;

        public InvoiceController(TimeTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("month/{monthDate}"), AllowAnonymous] // anon temp
        public async Task<IActionResult> Month(string monthDate)
        {
            return View(model: new InvoiceDto());
        }
        
        
    }
}