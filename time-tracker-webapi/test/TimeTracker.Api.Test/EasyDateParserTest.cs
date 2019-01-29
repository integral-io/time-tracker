using System;
using FluentAssertions;
using Xunit;

namespace TimeTracker.Api.Test
{
    public class EasyDateParserTest
    {
        [Fact]
        public void ParseEasyDate_handlesStringJan21()
        {
            int year = DateTime.UtcNow.Year;
            
            var jan21 = EasyDateParser.ParseEasyDate("jan-21");
            jan21.Should().NotBeNull();
            jan21.Value.Day.Should().Be(21);
            jan21.Value.Month.Should().Be(1);
            jan21.Value.Year.Should().Be(year);
        }
        
        [Fact]
        public void ParseEasyDate_handlesStringDec9_2018()
        {
            var jan21 = EasyDateParser.ParseEasyDate("December-9-2018");
            jan21.Should().NotBeNull();
            jan21.Value.Day.Should().Be(9);
            jan21.Value.Month.Should().Be(12);
            jan21.Value.Year.Should().Be(2018);
        }
        
        [Fact]
        public void ParseEasyDate_returnNullForInvalidDateString()
        {
            var jan21 = EasyDateParser.ParseEasyDate("Dixembra-40");
            jan21.Should().BeNull();
        }
        
        
    }
}