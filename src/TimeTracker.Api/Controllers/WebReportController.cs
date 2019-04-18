using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [Route("user/web"), Authorize] // todo: change back to Authorize
    public class WebReportController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;

        public WebReportController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        [HttpGet("{userid}")]
        public async Task<ViewResult> UserEntryReport(Guid userid)
        {
            var webReportService = new WebReportService(dbContext);
            var items = await webReportService.GetUserReport(userid);
            return View(items);
        }
    }
}