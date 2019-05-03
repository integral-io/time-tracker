using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Data.Models;
using TimeTracker.Library.Models;

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

        public async Task<ICollection<ProjectRp>> GetAllProjects()
        {
            var query = from p in dbContext.Projects
                select new ProjectRp()
                {
                    Name = p.Name,
                    ProjectId = p.ProjectId
                };
            return await query.ToListAsync();
        }
    }
}