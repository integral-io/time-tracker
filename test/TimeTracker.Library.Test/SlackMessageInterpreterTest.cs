using System;
using System.Collections.Generic;
using FluentAssertions;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Interpretation;
using Xunit;

namespace TimeTracker.Library.Test
{
    public class SlackMessageInterpreterTest
    {
        #region record

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursTodayForProjectWFH()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record au 8 wfh"));
            
            // change timezone on sut. date, and then compare the Date portion only.
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kabul");

            var kabulTime = TimeZoneInfo.ConvertTimeFromUtc(sut.Date, tzi);

            sut.Hours.Should().Be(8d);
            sut.Project.Should().Be("au");
            sut.IsWorkFromHome.Should().BeTrue();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);

            kabulTime.Day.Should().Be(sut.Date.Day);
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursTodayWithWfh()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record au 8 wfh"));
            sut.Hours.Should().Be(8d);
            sut.IsWorkFromHome.Should().BeTrue();
            sut.Project.Should().Be("au");
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.BillableProject);
            sut.Date.Day.Should().Be(DateTime.UtcNow.Day);
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretHoursYesterdayForProject()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record au 8 yesterday"));
            sut.Date.Date.Should().Be(DateTime.UtcNow.AddDays(-1).Date);
            sut.Hours.Should().Be(8d);
        }

        [Fact]
        public void InterpretHoursRecordMessage_doesNotFullyInterpretGibberish()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record au 8 yesterday or maybe today"));
            sut.ErrorMessage.Should().Be("Not sure how to interpret 'or maybe today'");
        }

        [Theory]
        [InlineData("nonbill", "\"lunch and learn\"", 2, null)]
        [InlineData("nonbillable", "\"lunch and learn\"", 2, null)]
        [InlineData("nonbillable", "pda", 2, null)]
        [InlineData("nonbillable", "fsa", 4, "mar-21")]
        [InlineData("nonbill", "fsa", 4, "mar-21-2019")]
        [InlineData("nonbill", "mar-19", 4, "some non bill reason")]
        public void InterpretHoursRecordMessage_canInterpretNonBillableWithVarietyText(string nonbillText, string description, double hours, string dateText)
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload($"record {nonbillText} {hours:F1} {description} {dateText ?? ""}"));
            if (dateText != "some non bill reason")
            {
                sut.Date.Date.Should().Be(EasyDateParser.ParseEasyDate(dateText));
                sut.NonBillReason.Should().Be(description.Replace("\"", ""));
            }
            else
            {
                sut.Date.Date.Should().Be(EasyDateParser.ParseEasyDate(description));
                sut.NonBillReason.Should().Be(dateText.Replace("\"", ""));
            }
            
            sut.Hours.Should().Be(hours);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.NonBillable);
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretSickHours()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record sick 8 \"flu\""));
            sut.Date.Date.Should().Be(DateTime.UtcNow.Date);
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Sick);
            sut.NonBillReason.Should().Be("flu");
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretVacationHours()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record vacation 8"));
            sut.Date.Date.Should().Be(DateTime.UtcNow.Date);
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Vacation);
            sut.NonBillReason.Should().BeNullOrEmpty();
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretVacationSpecificDate()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record vacation 8 2019-01-10"));
            sut.Date.Date.Should().Be(new DateTime(2019,1,10,0,0,0, DateTimeKind.Utc));
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Vacation);
            sut.NonBillReason.Should().BeNullOrEmpty();
        }

        [Fact]
        public void InterpretHoursRecordMessage_canInterpretVacationEasyDate()
        {
            var sut = new HoursInterpreter().InterpretMessage(ToPayload("record vacation 8 jan-21"));
            sut.Date.Date.Should().Be(new DateTime(2019,1,21,0,0,0, DateTimeKind.Utc));
            sut.Hours.Should().Be(8d);
            sut.IsBillable.Should().BeFalse();
            sut.TimeEntryType.Should().Be(TimeEntryTypeEnum.Vacation);
            sut.NonBillReason.Should().BeNullOrEmpty();
        }

        #endregion

        #region report

        [Fact]
        public void InterpretReportMessage_canInterpretReportCurrentMonth()
        {
            var currentDate = DateTime.UtcNow;
            
            var sut = new ReportInterpreter().InterpretMessage(ToPayload("summary"));
            sut.Date.Year.Should().Be(currentDate.Year);
            sut.Date.Month.Should().Be(currentDate.Month);
        }

        #endregion

        #region projects

        [Fact]
        public void InterpretProjectsMessage_canInterprojects()
        {
            var currentDate = DateTime.UtcNow;
            
            var sut = new ProjectsInterpreter().InterpretMessage(ToPayload("projects"));
            sut.Projects.Should().BeNull();
            sut.Date.Year.Should().Be(currentDate.Year);
            sut.Date.Month.Should().Be(currentDate.Month);
        }

        #endregion

        #region delete

        [Fact]
        public void InterpretDeleteMessage_canInterpretDeleteCurrentDay()
        {
            var sut = new DeleteInterpreter().InterpretMessage(ToPayload("delete"));
            var now = DateTime.UtcNow;
            sut.Date.Year.Should().Be(now.Year);
            sut.Date.Month.Should().Be(now.Month);
            sut.Date.Day.Should().Be(now.Day);
            sut.TimeEntryType.HasValue.Should().BeFalse();
        }

        [Fact]
        public void InterpretDeleteMessage_canInterpretDeleteSpecificDay()
        {
            var sut = new DeleteInterpreter().InterpretMessage(ToPayload("delete jan-17"));
            var now = DateTime.UtcNow;
            sut.Date.Year.Should().Be(now.Year);
            sut.Date.Month.Should().Be(1);
            sut.Date.Day.Should().Be(17);
            sut.TimeEntryType.HasValue.Should().BeFalse();
        }
        
        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable)]
        [InlineData(TimeEntryTypeEnum.Sick)]
        [InlineData(TimeEntryTypeEnum.Vacation)]
        [InlineData(TimeEntryTypeEnum.BillableProject)]
        public void InterpretDeleteMessage_canInterpretDifferentTimeTypeDeletion(TimeEntryTypeEnum timeEntryType)
        {
            var sut = new DeleteInterpreter().InterpretMessage(ToPayload("delete " + timeEntryType.GetDescription()));
            var now = DateTime.UtcNow;
            sut.Date.Year.Should().Be(now.Year);
            sut.Date.Month.Should().Be(now.Month);
            sut.Date.Day.Should().Be(now.Day);
            sut.TimeEntryType.Should().Be(timeEntryType);
        }
                
        [Theory]
        [InlineData(TimeEntryTypeEnum.NonBillable)]
        [InlineData(TimeEntryTypeEnum.Sick)]
        [InlineData(TimeEntryTypeEnum.Vacation)]
        [InlineData(TimeEntryTypeEnum.BillableProject)]
        public void InterpretDeleteMessage_canInterpretDifferentTimeTypeAndDayDeletion(TimeEntryTypeEnum timeEntryType)
        {
            var sut = new DeleteInterpreter().InterpretMessage(ToPayload("delete " + timeEntryType.GetDescription() + " jan-17"));
            var now = DateTime.UtcNow;
            sut.Date.Year.Should().Be(now.Year);
            sut.Date.Month.Should().Be(1);
            sut.Date.Day.Should().Be(17);
            sut.TimeEntryType.Should().Be(timeEntryType);
        }
        #endregion

        #region web
        

        #endregion

        #region helpers

        [Fact]
        public void FindDatePart_findsDateText()
        {
            const string dateText = "jan-21";
            var list = new List<TextMessagePart>()
            {
                new TextMessagePart() { Text = dateText},
                new TextMessagePart() { Text = "wfh"},
                new TextMessagePart() { Text = "8"},
                new TextMessagePart() { Text = "au"},
                new TextMessagePart() { Text = "i lik toilets"}
            };
            var datePart = SlackMessageInterpreter.FindDatePart(list);
            datePart.Should().NotBeNull();
            datePart.Text.Should().Be(dateText);
        }

        [Fact]
        public void FindDatePart_findsYesterday()
        {
            const string dateText = "yesterday";
            var list = new List<TextMessagePart>()
            {
                new TextMessagePart() { Text = "wfh"},
                new TextMessagePart() { Text = dateText},
                new TextMessagePart() { Text = "8"},
                new TextMessagePart() { Text = "au", IsUsed = true},
                new TextMessagePart() { Text = "i lik toilets"}
            };
            var datePart = SlackMessageInterpreter.FindDatePart(list);
            datePart.Should().NotBeNull();
            datePart.Text.Should().Be(dateText);
        }

        #endregion

        private static SlashCommandPayload ToPayload(string text)
        {
            return new SlashCommandPayload
            {
                text = text
            };
        }
    }

}