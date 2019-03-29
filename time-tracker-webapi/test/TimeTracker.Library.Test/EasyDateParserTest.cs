using System;
using FluentAssertions;
using Xunit;

namespace TimeTracker.Library.Test
{
    public class EasyDateParserTest
    {
        [Fact]
        public void ParseEasyDate_handlesStringJan21()
        {
            var year = DateTime.UtcNow.Year;
            
            var jan21 = EasyDateParser.ParseEasyDate("jan-21");
            jan21.Day.Should().Be(21);
            jan21.Month.Should().Be(1);
            jan21.Year.Should().Be(year);
        }
        
        [Fact]
        public void ParseEasyDate_handlesStringDec9_2018()
        {
            var jan21 = EasyDateParser.ParseEasyDate("December-9-2018");
            jan21.Day.Should().Be(9);
            jan21.Month.Should().Be(12);
            jan21.Year.Should().Be(2018);
        }
        
        [Theory]
        [InlineData("Dixembra-40")]
        [InlineData("January-40")]
        [InlineData("Dec")]
        public void IsSupportedDate_returnFalseForInvalidDateString(string date)
        {
            var jan21 = EasyDateParser.IsSupportedDate(date);
            jan21.Should().BeFalse();
        }

        [Fact]
        public void ParseEasyDate_handlesISOFormatDate()
        {
            var jan21 = EasyDateParser.ParseEasyDate("2018-01-21");
            jan21.Day.Should().Be(21);
            jan21.Month.Should().Be(1);
            jan21.Year.Should().Be(2018);
        }
        
        [Fact]
        public void ParseEasyDate_handles_yesterday_text()
        {
            var yesterday = EasyDateParser.GetUtcNow().AddDays(-1);
            var dateToTest = EasyDateParser.ParseEasyDate("yesterday");
            dateToTest.Day.Should().Be(yesterday.Day);
            dateToTest.Month.Should().Be(yesterday.Month);
            dateToTest.Year.Should().Be(yesterday.Year);
        }
        
        [Fact]
        public void ParseEasyDate_handles_null()
        {
            var today = EasyDateParser.GetUtcNow();
            var dateToTest = EasyDateParser.ParseEasyDate(null);
            dateToTest.Day.Should().Be(today.Day);
            dateToTest.Month.Should().Be(today.Month);
            dateToTest.Year.Should().Be(today.Year);
        }
    }
}