namespace TimeTracker.Library.Models.Admin
{
    public class UserReport
    {
        public string SlackUserName { get; set; }
        public string First { get; set; }
        public string Last { get; set; }

        public double BillableHoursYtd { get; set; }
        public double SickHoursYtd { get; set; }
        public double VacationHoursYtd { get; set; }
        public double OtherNonBillableYtd { get; set; }
    }
}