namespace TimeTracker.Data.Models
{
    /// <summary>
    /// used to describe a time entry into groups. This would be used to mark entry as Sick or Vacation time for example.
    /// </summary>
    public enum TimeEntryTypeEnum
    {
        BillableProject = 0,
        NonBillable = 1,
        Sick = 5,
        Vacation = 6
    }
}