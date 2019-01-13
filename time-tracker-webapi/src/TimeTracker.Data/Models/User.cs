using System;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TimeTracker.Data.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string SlackUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}