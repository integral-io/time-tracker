using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using TimeTracker.Data;
using TimeTracker.Library.Models.Admin;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [Route("admin/reports"), Authorize]
    public class AdminReportsController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;
        private const int PayPeriodLength = 14;

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
        public async Task<ViewResult> PeriodReport(string start, string end = null, int? projectId = null)
        {
            if (projectId == 0)
            {
                projectId = null;
            }

            DateTime startDate = Convert.ToDateTime(start);
            DateTime endDate = string.IsNullOrEmpty(end) ? startDate.AddDays(PayPeriodLength) : Convert.ToDateTime(end);
            
            var projectService = new ProjectService(dbContext);
            var adminReportService = new AdminReportService(dbContext);
            var items = await adminReportService.GetAllUsersByDate(startDate, endDate, projectId);
            var projects = await projectService.GetAllProjects();
            
            var viewModel = new PayPeriodReportViewModel()
            {
                PayPeriodStartDate = startDate,
                PayPeriodEndDate = endDate,
                ReportItems = items.ToImmutableList(),
                Projects = projects.ToImmutableList()
            };
            return View(viewModel);
        }
    }
}