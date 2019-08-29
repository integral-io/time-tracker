using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;
using TimeTracker.Library.Models.WebReport;

namespace TimeTracker.Api.Models
{
    public class UserRecordHoursViewModel
    {
        public ImmutableList<UserEntry> Hours { get; set; }
        public ImmutableList<ProjectRp> Projects { get; set; }
        public TimeEntryTypeEnum TimeEntryType { get; set; }

        public int? ProjectId { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public ImmutableList<SelectListItem> Months { get; set; }
        public string SelectedMonth { get; set; }

        public TotalHourSummary TotalYearly { get; set; }
        
        public TotalHourSummary TotalMonthly { get; set; }
    }
}