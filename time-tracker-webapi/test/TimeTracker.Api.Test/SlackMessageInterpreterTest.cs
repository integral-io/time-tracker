using System;
using FluentAssertions;
using Xunit;

namespace TimeTracker.Api.Test
{
    public class SlackMessageInterpreterTest
    {
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
        public void InterpretReportMessage_canInterpretReportForMonth()
        {
            String projectName = "validprojectname";
            var sut = SlackMessageInterpreter.InterpretReportMessage($"report {projectName}");
            sut.Project.Should().Be(projectName);
            sut.Month.Should().Be(DateTime.UtcNow.ToString("MMMM"));
        }
    }
}