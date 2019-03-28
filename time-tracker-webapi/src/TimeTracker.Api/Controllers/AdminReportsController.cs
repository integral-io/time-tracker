using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Models.Admin;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [Route("admin/reports")]
    public class AdminReportsController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;

        public AdminReportsController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /* anonymous until figure out auth with google */
        [HttpGet("ytd"), AllowAnonymous, ProducesResponseType(typeof(IEnumerable<UserReport>), 200)]
        public async Task<IActionResult> AllUsersReport()
        {
            var adminReportService = new AdminReportService(dbContext);
            var items = await adminReportService.GetAllUsersReport();
            return View(model: items);
        }
        
        [HttpGet("payperiod"), AllowAnonymous, ProducesResponseType(typeof(IEnumerable<UserReport>), 200)]
        public async Task<IActionResult> PeriodReport(string start, string end)
        {
            var adminReportService = new AdminReportService(dbContext);
            var items = await adminReportService.GetAllUsersReport();
            return View(model: items);
        }
    }
}