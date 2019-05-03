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
    [Route("user/web"), Authorize(Roles = AppRoles.OrganizationMember)]
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
        public async Task<ViewResult> UserEntryReport()
        {
            var ident = User.Identity as ClaimsIdentity;
            // refactor this line, also used in AccountController
            string googleId = ident.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            Guid userId = await userService.GetUserIdFromGoogleId(googleId);
            
            var webReportService = new WebReportService(dbContext);
            var items = await webReportService.GetUserReport(userId);
            return View(items);
        }
    }
}