using System;
using System.Data;

namespace TimeTracker.Data.Models
{
    /// <summary>
    /// defines projects people are working on. Optionally can define a client for the project
    /// </summary>
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? BillingClientId { get; set; }
    }
}