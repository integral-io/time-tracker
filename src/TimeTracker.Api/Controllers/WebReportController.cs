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
    [Route(""), Authorize]
    public class WebReportController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;
        private readonly UserService userService;

        public WebReportController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
            userService = new UserService(dbContext);
        }
        
        [HttpGet("")]
        public async Task<ViewResult> Index(string selectedMonth = null)
        {
            var userId = await GetUserId();
            
            DateTime? selectedDate = null;

            if (!String.IsNullOrEmpty(selectedMonth) && DateTime.TryParse(selectedMonth, out DateTime selectedDate2))
            {
                selectedDate = selectedDate2;
            }

            var webReportService = new WebReportService(dbContext);
            var projectService = new ProjectService(dbContext);
            var items = await webReportService.GetUserReport(userId, selectedDate?.Month);

            var model = new UserRecordHoursViewModel()
            {
                Hours = items.ToImmutableList(),
                TimeEntryType = TimeEntryTypeEnum.BillableProject,
                Projects = (await projectService.GetAllProjects()).ToImmutableList(),
                Name = items.Any() ? items.First().Name : string.Empty,
                Date = DateTime.UtcNow.Date,
                Months = await webReportService.GetUserAvailableMonths(userId),
                SelectedMonth = selectedMonth
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

            return RedirectToAction("Index");
        }

        private async Task<Guid> GetUserId()
        {
            var ident = User.Identity as ClaimsIdentity;
            string googleId = ident.Claims
                .First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            Guid userId = await userService.GetUserIdFromGoogleId(googleId);
            return userId;
        }
    }
}