using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var projectService = new ProjectService(dbContext);
            var items = await webReportService.GetUserReport(userId);

            var model = new UserRecordHoursViewModel()
            {
                Hours = items.ToImmutableList(),
                TimeEntryType = TimeEntryTypeEnum.BillableProject,
                Projects = (await projectService.GetAllProjects()).ToImmutableList(),
                Name = items.Any() ? items.First().Name : string.Empty,
                Date = DateTime.UtcNow.Date
            };
            
            return View(model);
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordHours(RecordHoursPost model)
        {
            var userId = await GetUserId();
            TimeEntryService timeEntryService = new TimeEntryService(userId, this.dbContext);

            DateTime date = DateTime.Parse(model.Date);

            if (model.TimeEntryType == TimeEntryTypeEnum.BillableProject)
            {
                await timeEntryService.CreateBillableTimeEntry(date, model.Hours, model.ProjectId.Value);
            }
            else
            {
                await timeEntryService.CreateNonBillableTimeEntry(date, model.Hours, model.NonbillReason,
                    model.TimeEntryType);
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