namespace TimeTracker.Data.Models
{
    /// <summary>
    /// used to describe a time entry into groups. This would be used to mark entry as Sick or Vacation time for example.
    /// </summary>
    public class TimeEntryType
    {
        public int TimeEntryTypeId { get; set; }

        public bool IsBillable { get; set; }

        public string Name { get; set; }
    }
}