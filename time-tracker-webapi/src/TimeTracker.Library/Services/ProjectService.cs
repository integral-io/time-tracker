using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;

namespace TimeTracker.Library.Services
{
    public class ProjectService
    {
        private readonly Guid _userId;
        private readonly TimeTrackerDbContext _dbContext;

        public ProjectService(in Guid userId, in TimeTrackerDbContext dbContext)
        {
            _userId = userId;
            _dbContext = dbContext;
        }

        public async Task<Project> FindProjectFromName(string projectName)
        {
            // todo: handle filtering by assigned users (optional feature)?
            return await _dbContext.Projects.FirstOrDefaultAsync(x =>
                x.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}