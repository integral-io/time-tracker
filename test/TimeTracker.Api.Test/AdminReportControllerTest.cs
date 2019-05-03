using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Api.Controllers;
using TimeTracker.Library.Models.Admin;
using TimeTracker.TestInfra;
using Xunit;

namespace TimeTracker.Api.Test
{
    public class AdminReportControllerTest
    {
        [Fact]
        public async Task PeriodReport_returnsExpectedModelWithEndDate_whenStartDateSet()
        {
            var database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            database.AddTimeOff();
            
            var controller = new AdminReportsController(database);

            var expectedStartDate = database.TimeEntries.First().Date;
            var expectedEndDate = expectedStartDate.AddDays(14);
            var result = await controller.FilteredReport(expectedStartDate.ToString("yyyy-MM-dd"));

            var model = result.Model.Should().BeOfType<PayPeriodReportViewModel>().Which;
            model.SelectedProjectId.Should().BeNull();
            model.PayPeriodStartDate.Date.Should().Be(expectedStartDate.Date);
            model.PayPeriodEndDate.Date.Should().Be(expectedEndDate.Date);
            model.ReportItems.Should().HaveCount(database.Users.Count());
        }
        
        [Fact]
        public async Task PeriodReport_returnsModelWithStartAndEndForCurrentMonth_whenNoDates()
        {
            var database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            database.AddTimeOff();
            
            var controller = new AdminReportsController(database);

            var expectedStartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var expectedEndDate = expectedStartDate.AddMonths(1).AddDays(-1);
            var result = await controller.FilteredReport();

            var model = result.Model.Should().BeOfType<PayPeriodReportViewModel>().Which;
            model.SelectedProjectId.Should().BeNull();
            model.PayPeriodStartDate.Date.Should().Be(expectedStartDate.Date);
            model.PayPeriodEndDate.Date.Should().Be(expectedEndDate.Date);
            model.ReportItems.Should().HaveCount(database.Users.Count());
        }
    }
}