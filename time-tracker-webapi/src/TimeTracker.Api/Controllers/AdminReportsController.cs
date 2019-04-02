using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Models.Admin;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [Route("admin/reports"), Authorize]
    public class AdminReportsController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;

        public AdminReportsController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("ytd")]
        public async Task<ViewResult> AllUsersReport()
        {
            var adminReportService = new AdminReportService(dbContext);
            var items = await adminReportService.GetAllUsersReport();
            return View(items);
        }
        
        [HttpGet("payperiod")]
        public async Task<ViewResult> PeriodReport(string start, string end)
        {
            var adminReportService = new AdminReportService(dbContext);
            var items = await adminReportService.GetAllUsersReport();
            return View(items);
        }
    }
}