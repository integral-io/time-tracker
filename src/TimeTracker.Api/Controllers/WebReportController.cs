using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Models;
using TimeTracker.Data;
using TimeTracker.Data.Models;
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
        public async Task<ViewResult> UserEntryReport()
        {
            var userId = await GetUserId();

            var webReportService = new WebReportService(dbContext);
            var items = await webReportService.GetUserReport(userId);
            return View(items);
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordHours(RecordHoursPost model)
        {
            var userId = await GetUserId();
            TimeEntryService timeEntryService = new TimeEntryService(userId, this.dbContext);

            if (model.TimeEntryType == TimeEntryTypeEnum.BillableProject)
            {
               // timeEntryService.CreateBillableTimeEntry()
            }
            else
            {
                
            }

            return RedirectToAction("UserEntryReport");
        }

        private async Task<Guid> GetUserId()
        {
            var ident = User.Identity as ClaimsIdentity;
            // refactor this line, also used in AccountController
            string googleId = ident.Claims
                .First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            Guid userId = await userService.GetUserIdFromGoogleId(googleId);
            return userId;
        }
    }
}