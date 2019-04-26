using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Should not be edited by user. Claim: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
        /// </summary>
        public string GoogleIdentifier { get; set; }

        /// <summary>
        /// Should not be edited by user. Claim: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress
        /// </summary>
        public string OrganizationEmail { get; set; }

        public ICollection<TimeEntry> TimeEntries { get; set; }
    }
}