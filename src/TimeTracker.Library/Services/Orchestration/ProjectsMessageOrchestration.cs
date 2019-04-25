  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using TimeTracker.Data;
  using TimeTracker.Library.Services.Interpretation;

  namespace TimeTracker.Library.Services.Orchestration
    {
        public class ProjectsMessageOrchestration : MessageOrchestration<ProjectsInterpreter, ProjectsInterpretedMessage>
        {
            private readonly TimeTrackerDbContext dbContext;

            public ProjectsMessageOrchestration(TimeTrackerDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            protected override async Task<SlackMessageResponse> RespondTo(ProjectsInterpretedMessage message)
            {
                var projectService = new ProjectService(dbContext);
                var projects = dbContext.Projects.ToList(); 

                var stringBuilder = new StringBuilder();


                foreach (var project in projects)
                {

                    stringBuilder.Append(project.Name + " - " + project.BillingClient.Name + projects.Count());
                }    
                
                return new SlackMessageResponse(stringBuilder.ToString(), true);
            }
        }
    }
