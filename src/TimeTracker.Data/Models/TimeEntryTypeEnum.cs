using System.ComponentModel;
using System.Linq;

namespace TimeTracker.Data.Models
{
    /// <summary>
    /// used to describe a time entry into groups. This would be used to mark entry as Sick or Vacation time for example.
    /// </summary>
    public enum TimeEntryTypeEnum
    {
        [Description("billable")]
        BillableProject = 0,
        
        [Description("nonbill")]
        NonBillable = 1,
        
        [Description("sick")]
        Sick = 5,
        
        [Description("vacation")]
        Vacation = 6
    }

    public static class EnumDescription
    {
        public static string GetDescription(this TimeEntryTypeEnum type)
        {
            var memberInfos = type.GetType().GetMember(type.ToString());
            var attribute = memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute) attribute.ElementAt(0)).Description;
            return description;
        }
    }
}