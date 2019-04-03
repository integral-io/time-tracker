using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Library.Services
{
    public class ProjectService
    {
        private readonly TimeTrackerDbContext dbContext;

        public ProjectService(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Project> FindProjectFromName(string projectName)
        {
            // todo: handle filtering by assigned users (optional feature)?
            return await dbContext.Projects.FirstOrDefaultAsync(x =>
                x.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}