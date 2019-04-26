using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Library.Services.Orchestration;
using Xunit;

namespace TimeTracker.Library.Test.Services.Orchestration
{
    public class ReportTests
    {
        private readonly TimeTrackerDbContext database;
        private readonly SlackMessageOrchestrator orchestrator;
        private const string WebAppUri = "https://localhost";


        public ReportTests()
        {
            database = new InMemoryDatabaseWithProjectsAndUsers().Database;
            orchestrator = new SlackMessageOrchestrator(database, WebAppUri);
        }

        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable, "Other Non-billable")]
        [InlineData(TimeEntryTypeEnum.Sick, "Sick")]
        [InlineData(TimeEntryTypeEnum.Vacation, "Vacation")]
        [InlineData(TimeEntryTypeEnum.BillableProject, "Billable")]
        public async Task WhenTimeEntryStoredOnFirstOfMonth_ThenItGetsDisplayedInReportCurrentMonth(TimeEntryTypeEnum entryType, string reportText)
        {
            var user = database.Users.First();
            var now = DateTime.UtcNow;
            await StoreTimeEntryOnFirstDayOfMonth(entryType, user, 8, now.Month);
            
            var response = await RequestReport(user);
            
            response.Text.Should().Contain($"{now:MMMM yyyy} {reportText} Hours: 8.0");
        }

        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable, "Other Non-billable")]
        [InlineData(TimeEntryTypeEnum.Sick, "Sick")]
        [InlineData(TimeEntryTypeEnum.Vacation, "Vacation")]
        [InlineData(TimeEntryTypeEnum.BillableProject, "Billable")]
        public async Task WhenTimeEntryStoredOnFirstDayOfYear_ThenItGetsDisplayedInReportYtd(TimeEntryTypeEnum entryType, string reportText)
        {
            var user = database.Users.First();
            await StoreTimeEntryOnFirstDayOfMonth(entryType, user, 8, 1);

            var response = await RequestReport(user);

            response.Text.Should().Contain($"{DateTime.UtcNow.Year} Total {reportText} Hours: 8.0");
        }
        
        [Fact]
        public async Task WhenRequestingNonSpecificReport_ReportIncludesCurrentWeekSummary()
        {
            DateTime date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month - 1, DateTime.UtcNow.Day);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);

            date = date.AddMonths(1);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);            

            
            DateTime mayDate = new DateTime(DateTime.UtcNow.Year, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"This Week Billable Hours: 2.0");
            response.Text.Should().Contain($"This Week Vacation Hours: 3.0");
            response.Text.Should().Contain($"This Week Sick Hours: 1.0");
            response.Text.Should().Contain($"This Week Other Non-billable Hours: 0.0");
        }

        [Fact]
        public async Task WhenRequestingReportForSpecificMonth_ReportOnlyIncludesHoursForTheMonth()
        {
            DateTime date = new DateTime(DateTime.UtcNow.Year, 3, 18);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);            
            
            DateTime mayDate = new DateTime(DateTime.UtcNow.Year, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report month mar",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"March {date.Year} Billable Hours: 2.0");
            response.Text.Should().Contain($"March {date.Year} Vacation Hours: 3.0");
            response.Text.Should().Contain($"March {date.Year} Sick Hours: 1.0");
            response.Text.Should().Contain($"March {date.Year} Other Non-billable Hours: 0.0");
        }
        
        [Fact]
        public async Task WhenRequestingReportForSpecificYear_ReportIncludesAllHoursForThatYear()
        {
            DateTime date = new DateTime(2018, 1, 1);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);            
            
            DateTime mayDate = new DateTime(2019, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report year 2018",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"{date.Year} Total Billable Hours: 2.0");
            response.Text.Should().Contain($"{date.Year} Total Vacation Hours: 3.0");
            response.Text.Should().Contain($"{date.Year} Total Sick Hours: 1.0");
            response.Text.Should().Contain($"{date.Year} Total Other Non-billable Hours: 0.0");
        }
        
        [Fact]
        public async Task WhenRequestingReportForSpecificMonthAndYear_ReportIncludesAllHoursForThatMonthAndYear()
        {
            DateTime date = new DateTime(2018, 2, 1);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);            
            
            DateTime mayDate = new DateTime(2019, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report month feb 2018",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"February {date.Year} Billable Hours: 2.0");
            response.Text.Should().Contain($"February {date.Year} Vacation Hours: 3.0");
            response.Text.Should().Contain($"February {date.Year} Sick Hours: 1.0");
            response.Text.Should().Contain($"February {date.Year} Other Non-billable Hours: 0.0");
        }
        
              
        [Theory]
        [InlineData("feb-2018")]
        [InlineData("Feb-2018")]
        [InlineData("FEB-2018")]
        [InlineData("feb 2018")]
        [InlineData("Feb 2018")]
        [InlineData("FEB 2018")]
        [InlineData("Febr 2018")]
        [InlineData("Febr-2018")]
        [InlineData("FEBRUARY 2018")]
        [InlineData("FEBRUARY-2018")]
        [InlineData("February 2018")]
        [InlineData("February-2018")]
        public async Task CanRequestReportForSpecificMonthAndYearWithoutDashParsingIssues(string dateEntry)
        {
            DateTime date = new DateTime(2018, 2, 1);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(1), 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date.AddDays(2), 1, "flu", TimeEntryTypeEnum.Sick);            
            
            DateTime mayDate = new DateTime(2019, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report month " + dateEntry,
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"February {date.Year} Billable Hours: 2.0");
            response.Text.Should().Contain($"February {date.Year} Vacation Hours: 3.0");
            response.Text.Should().Contain($"February {date.Year} Sick Hours: 1.0");
            response.Text.Should().Contain($"February {date.Year} Other Non-billable Hours: 0.0");
        }
        
        [Fact]
        public async Task WhenRequestingReportForSpecificDate_ReportIncludesAllHoursForThatDay()
        {
            DateTime date = new DateTime(2018, 2, 9);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(date, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(date, 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(date, 1, "flu", TimeEntryTypeEnum.Sick);            
            
            DateTime mayDate = new DateTime(2019, 5, 18);
            await timeEntryService.CreateBillableTimeEntry(mayDate, 2, 1, 1);
            
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report date Feb-9-2018",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
            
            response.Text.Should().Contain($"February {date.Day} {date.Year} Billable Hours: 2.0");
            response.Text.Should().Contain($"February {date.Day} {date.Year} Vacation Hours: 3.0");
            response.Text.Should().Contain($"February {date.Day} {date.Year} Sick Hours: 1.0");
            response.Text.Should().Contain($"February {date.Day} {date.Year} Other Non-billable Hours: 0.0");
        }

        [Fact]

        public async Task WhenRequestingReportForLastTenEntries_ReportIncludesAListOfTenEntries()
        {
            DateTime yesterday = DateTime.UtcNow.AddDays(-1);
            var user = database.Users.First();
            TimeEntryService timeEntryService = new TimeEntryService(user.UserId, database);
            await timeEntryService.CreateBillableTimeEntry(yesterday, 2, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(yesterday, 3, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(yesterday, 1, "flu", TimeEntryTypeEnum.Sick);

            DateTime twoDaysAgo = yesterday.AddDays(-1);
            await timeEntryService.CreateBillableTimeEntry(twoDaysAgo, 4, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(twoDaysAgo, 2, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(twoDaysAgo, 2, "flu", TimeEntryTypeEnum.Sick);

            DateTime threeDaysAgo = twoDaysAgo.AddDays(-1);
            await timeEntryService.CreateBillableTimeEntry(threeDaysAgo, 5, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(threeDaysAgo, 1, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(threeDaysAgo, 2, "flu", TimeEntryTypeEnum.Sick); 
            
            DateTime fourDaysAgo = threeDaysAgo.AddDays(-1);
            await timeEntryService.CreateBillableTimeEntry(fourDaysAgo, 1, 1, 1);
            await timeEntryService.CreateNonBillableTimeEntry(fourDaysAgo.AddMinutes(-30), 4, null, TimeEntryTypeEnum.Vacation);
            await timeEntryService.CreateNonBillableTimeEntry(fourDaysAgo.AddMinutes(-15), 2, "flu", TimeEntryTypeEnum.Sick); 
            
            var response = await orchestrator.HandleCommand(new SlashCommandPayload
            {    
                text = "report last",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });

            response.Text.Should().Contain("The Last Ten Time Entries: ").And
                .Contain(
                    $"{yesterday.Month}-{yesterday.Day}-{yesterday.Year} {TimeEntryTypeEnum.BillableProject.GetDescription()} 2 hours")
                .And
                .Contain(
                    $"{yesterday.Month}-{yesterday.Day}-{yesterday.Year} {TimeEntryTypeEnum.Vacation.GetDescription()} 3 hours")
                .And
                .Contain(
                    $"{yesterday.Month}-{yesterday.Day}-{yesterday.Year} {TimeEntryTypeEnum.Sick.GetDescription()} 1 hours")
                .And
                .Contain(
                    $"{twoDaysAgo.Month}-{twoDaysAgo.Day}-{twoDaysAgo.Year} {TimeEntryTypeEnum.BillableProject.GetDescription()} 4 hours")
                .And
                .Contain(
                    $"{twoDaysAgo.Month}-{twoDaysAgo.Day}-{twoDaysAgo.Year} {TimeEntryTypeEnum.Vacation.GetDescription()} 2 hours")
                .And
                .Contain(
                    $"{twoDaysAgo.Month}-{twoDaysAgo.Day}-{twoDaysAgo.Year} {TimeEntryTypeEnum.Sick.GetDescription()} 2 hours")
                .And
                .Contain(
                    $"{threeDaysAgo.Month}-{threeDaysAgo.Day}-{threeDaysAgo.Year} {TimeEntryTypeEnum.BillableProject.GetDescription()} 5 hours")
                .And
                .Contain(
                    $"{threeDaysAgo.Month}-{threeDaysAgo.Day}-{threeDaysAgo.Year} {TimeEntryTypeEnum.Vacation.GetDescription()} 1 hours")
                .And
                .Contain(
                    $"{threeDaysAgo.Month}-{threeDaysAgo.Day}-{threeDaysAgo.Year} {TimeEntryTypeEnum.Sick.GetDescription()} 2 hours")
             
                .And
                .Contain(
                    $"{fourDaysAgo.Month}-{fourDaysAgo.Day}-{fourDaysAgo.Year} {TimeEntryTypeEnum.BillableProject.GetDescription()} 1 hours");


        }
        private Task<SlackMessage> RequestReport(User user)
        {
            return orchestrator.HandleCommand(new SlashCommandPayload
            {
                text = "report",
                user_id = user.SlackUserId,
                user_name = user.UserName
            });
        }

        private async Task StoreTimeEntryOnFirstDayOfMonth(TimeEntryTypeEnum entryType, User user, int hours, int month)
        {
            var timeEntry = new TimeEntry
            {
                Hours = hours,
                Date = new DateTime(DateTime.UtcNow.Year, month, 1),
                TimeEntryType = entryType,
                User = user
            };

            database.TimeEntries.Add(timeEntry);
            await database.SaveChangesAsync();
        }
        
    }
    
    
}