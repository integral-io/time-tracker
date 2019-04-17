using FluentAssertions;
using TimeTracker.Data.Models;
using Xunit;

namespace TimeTracker.Library.Test
{
    public class TimeEntryEnumDescriptionTest
    {
        [Theory]
        [InlineData(TimeEntryTypeEnum.BillableProject, "billable")]
        [InlineData(TimeEntryTypeEnum.NonBillable, "nonbill")]
        [InlineData(TimeEntryTypeEnum.Sick, "sick")]
        [InlineData(TimeEntryTypeEnum.Vacation, "vacation")]
        public void GetDescription_ReturnsMatchingStringToEnum(TimeEntryTypeEnum type, string name)
        {
            type.GetDescription().Should().Be(name);
        }
    }
}