using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TimeTracker.Data.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        /// <summary>
        /// Will normally be slack username
        /// </summary>
        public string UserName { get; set; }
        public string SlackUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<TimeEntry> TimeEntries { get; set; }
    }
}