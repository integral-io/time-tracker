using System;
using FluentAssertions;
using TimeTracker.Data.Models;
using Xunit;

namespace TimeTracker.Library.Test
{
    public class SlackMessageInterpreterTest
    {
        #region record
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursTodayForProjectWFH()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record au 8 wfh");
            
            // change timezone on sut. date, and then compare the Date portion only.
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kabul");

            DateTime kabulTime = TimeZoneInfo.ConvertTimeFromUtc(sut.Date, tzi);

            sut.Hours.Should().Be(8d);
            sut.Project.Should().Be("au");
            sut.IsWorkFromHome.Should().BeTrue();

            kabulTime.Day.Should().Be(sut.Date.Day);
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursTodayWithWfh()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record au 8 wfh");
            sut.Hours.Should().Be(8d);
            sut.IsWorkFromHome.Should().BeTrue();
            sut.Project.Should().Be("au");
            sut.Date.Day.Should().Be(DateTime.UtcNow.Day);
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
        
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretVacationSpecificDate()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record vacation 8 2019-01-10");
            sut.Date.Date.Should().Be(new DateTime(2019,1,10,0,0,0, DateTimeKind.Utc));
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Vacation);
            sut.NonBillReason.Should().BeNull();
        }
        
        [Fact]
        public void InterpretHoursRecordMessage_canInterpretVacationEasyDate()
        {
            var sut = SlackMessageInterpreter.InterpretHoursRecordMessage("record vacation 8 jan-21");
            sut.Date.Date.Should().Be(new DateTime(2019,1,21,0,0,0, DateTimeKind.Utc));
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Vacation);
            sut.NonBillReason.Should().BeNull();
        }
        
        #endregion
        
        #region report

        [Fact]
        public void InterpretReportMessage_canInterpretReportCurrentMonth()
        {
            var currentDate = DateTime.UtcNow;
            
            var sut = SlackMessageInterpreter.InterpretReportMessage("report");
            sut.Project.Should().BeNull();
            sut.Date.Year.Should().Be(currentDate.Year);
            sut.Date.Month.Should().Be(currentDate.Month);
        }
        
        #endregion

        #region delete

        [Fact]
        public void InterpretDeleteMessage_canInterpretDeleteCurrentDay()
        {
            var sut = SlackMessageInterpreter.InterpretDeleteMessage("delete");
            var now = DateTime.UtcNow;
            sut.Date.Year.Should().Be(now.Year);
            sut.Date.Month.Should().Be(now.Month);
            sut.Date.Day.Should().Be(now.Day);
        }
        
        [Fact]
        public void InterpretDeleteMessage_canInterpretDeleteSpecificDay()
        {
            var sut = SlackMessageInterpreter.InterpretDeleteMessage("delete jan-17");
            var now = DateTime.UtcNow;
            sut.Date.Year.Should().Be(now.Year);
            sut.Date.Month.Should().Be(1);
            sut.Date.Day.Should().Be(17);
        }

        #endregion
    }
}