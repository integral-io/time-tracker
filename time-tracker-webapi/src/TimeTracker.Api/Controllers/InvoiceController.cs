using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.Api.Controllers
{
    [Route("invoice")]
    public class InvoiceController : Controller
    {
        [HttpGet("month/{monthDate}"), AllowAnonymous] // anon temp
        public async Task<IActionResult> Month(string monthDate)
        {
            return View();
        }
    }
}