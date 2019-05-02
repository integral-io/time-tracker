using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.Api.Controllers
{
    [Route(""), Authorize]
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("claims")]
        public IActionResult Claims()
        {
            var ident = User.Identity as ClaimsIdentity;
            
            return View(ident.Claims);
        }
    }
}