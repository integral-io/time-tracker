using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;
using TimeTracker.Library.Models;

namespace TimeTracker.Api.Controllers
{
    [Route("invoice")]
    public class InvoiceController : Controller
    {
        [HttpGet("month/{monthDate}"), AllowAnonymous] // anon temp
        public async Task<IActionResult> Month(string monthDate)
        {
            return View(model: new InvoiceDto());
        }
        
        
        [HttpGet("adminreport"), AllowAnonymous] // anon temp
        public async Task<IActionResult> AdminTimeOffReport()
        {
            return View(model: new AllTimeOffDto());
        }
        
        
    }
}