using System;
using FluentAssertions;
using TimeTracker.Library.Services.Interpretation;
using Xunit;

namespace TimeTracker.Library.Test
{
    public class MonthConverterTest
    {
        [Theory]
        [InlineData("January", 1)]
        [InlineData("February", 2)]
        [InlineData("March", 3)]
        [InlineData("April", 4)]
        [InlineData("May", 5)]
        [InlineData("June", 6)]
        [InlineData("July", 7)] 
        [InlineData("August", 8)]
        [InlineData("September", 9)]
        [InlineData("October", 10)]
        [InlineData("November", 11)]
        [InlineData("December", 12)]
        public void AllStandardMonthAbbreviationsAndCasesAreSupported(string month, int monthNum)
        {
            month.ToMonth().Should().Be(monthNum);
            month.Substring(0, 3).ToMonth().Should().Be(monthNum);
            month.ToLower().Substring(0, 3).ToMonth().Should().Be(monthNum);
            month.ToUpper().Substring(0, 3).ToMonth().Should().Be(monthNum);
            month.ToLower().ToMonth().Should().Be(monthNum);
            month.ToUpper().ToMonth().Should().Be(monthNum);
        }
        
        [Fact]
        public void UnsupportedMonthEntryThrowsException()
        {
            Assert.Throws<Exception>(() => "jkhdfk".ToMonth());
        }
    }
}