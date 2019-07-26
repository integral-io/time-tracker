using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Controllers;
using TimeTracker.Api.Models;
using TimeTracker.TestInfra;
using Xunit;

namespace TimeTracker.Api.Test
{
    public class WebReportControllerTest 
    {
        [Theory]
        [InlineData(null)]
        [InlineData("2019-07-01")]
        [InlineData("2019-07")]
        public async Task Index_returnsModelWithExpectedDataForSelectedMonth(string selectedMonth)
        {
            var database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            database.AddTimeOff();

            var identityClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                database.Users.First().GoogleIdentifier);

            var controller = new WebReportController(database);
            // setup user mocking
            var claimsIdentity = new ClaimsIdentity(new List<Claim>() { identityClaim });
            var user = new ClaimsPrincipal(claimsIdentity);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var result = await controller.Index(selectedMonth);
            var model = result.Model.Should().BeOfType<UserRecordHoursViewModel>().Which;

            model.SelectedMonth.Should().Be(selectedMonth);
            model.Date.Should().Be(DateTime.UtcNow.Date);
            // todo: test that the model entries are what is expected per setup data
        }
    }
}