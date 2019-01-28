using System;
using FluentAssertions;
using TimeTracker.Data.Models;
using Xunit;

namespace TimeTracker.Api.Test
{
    public class SlackMessageInterpreterTest
    {
        #region record
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursTodayForProjectWFH()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record au 8 wfh");
            sut.Hours.Should().Be(8d);
            sut.Project.Should().Be("au");
            sut.IsWorkFromHome.Should().BeTrue();
        }
        
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursYesterdayForProject()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record au 8 yesterday");
            sut.Date.Date.Should().Be(DateTime.UtcNow.AddDays(-1).Date);
            sut.Hours.Should().Be(8d);
        }
        
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursTodayForNonBillable()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record nonbill 2 \"lunch and learn\"");
            sut.Date.Date.Should().Be(DateTime.UtcNow.Date);
            sut.Hours.Should().Be(2d);
            sut.IsBillable.Should().BeFalse();
            sut.NonBillReason.Should().Be("lunch and learn");
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretSickHours()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record sick 8 \"flu\"");
            sut.Date.Date.Should().Be(DateTime.UtcNow.Date);
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Sick);
            sut.NonBillReason.Should().Be("flu");
        }
        
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretVacationHours()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record vacation 8");
            sut.Date.Date.Should().Be(DateTime.UtcNow.Date);
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Vacation);
            sut.NonBillReason.Should().BeNull();
        }
        
        #endregion
        
        #region report

        [Fact]
        public void InterpretReportMessage_canInterpretReportForCurrentMonthAndProject()
        {
            String projectName = "validprojectname";
            var sut = SlackMessageInterpreter.InterpretReportMessage($"report {projectName}");
            sut.Project.Should().Be(projectName);
            var utcNow = DateTime.UtcNow;
            var expected = new DateTime(utcNow.Year, utcNow.Month, 1,0,0,0,DateTimeKind.Utc);
            
            sut.StartDateMonth.Should().Be(expected);
        }
        
        [Fact]
        public void InterpretReportMessage_canInterpretReportForSpecificMonth()
        {
            String projectName = "validprojectname";
            var sut = SlackMessageInterpreter.InterpretReportMessage($"report {projectName} 2018-12");
            sut.Project.Should().Be(projectName);
            
            var expected = new DateTime(2018, 12, 1,0,0,0,DateTimeKind.Utc);
            
            sut.StartDateMonth.Should().Be(expected);
        }
        
        [Fact]
        public void InterpretReportMessage_canInterpretReportWithDefaults()
        {
            String projectName = "validprojectname";
            var sut = SlackMessageInterpreter.InterpretReportMessage($"report");
            sut.Project.Should().Be(null);
            var utcNow = DateTime.UtcNow;
            var expected = new DateTime(utcNow.Year, utcNow.Month, 1,0,0,0,DateTimeKind.Utc);

            sut.StartDateMonth.Should().Be(expected);
        }
        
        #endregion
    }
}