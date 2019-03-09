using System.Collections.Generic;
using TimeTracker.Data;

namespace TimeTracker.Library.Services
{
    /// <summary>
    /// Reporting service for admin users to use that provide all user data
    /// </summary>
    public class AdminReportService
    {
        private readonly TimeTrackerDbContext _db;

        public AdminReportService(in TimeTrackerDbContext db)
        {
            _db = db;
        }

        public IReadOnlyCollection<UserReport> GetAllUsers()
        {
            return new List<UserReport>();
        }
    }
}