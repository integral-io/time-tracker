using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [Route("user/web"), Authorize]
    public class WebReportController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;
        private readonly UserService userService;

        public WebReportController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
            userService = new UserService(dbContext);
        }
        
        [HttpGet("userEntryReport")]
        private async Task<ViewResult> UserEntryReport()
        {
            var ident = User.Identity as ClaimsIdentity;
            // refactor this line, also used in AccountController
            string googleId = ident.Claims.First(x => x.Value == "Identifier").Value;
            // we need something to convert from google id to our UserId, and then some kind of caching layer,
            // or inherit from a master Controller that handles it under the covers. 
            Guid userId = userService.GetUserIdFromGoogleId(googleId);
            
            var webReportService = new WebReportService(dbContext);
            var items = await webReportService.GetUserReport(userId);
            return View(items);
        }
    }
}