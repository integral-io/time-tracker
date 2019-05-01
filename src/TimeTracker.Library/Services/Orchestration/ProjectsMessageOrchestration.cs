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
                var projects = dbContext.Projects.ToList(); 
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("Current Projects:");
                foreach (var project in projects)
                {
                    string client = "no client";
                    if (project.BillingClient != null && project.BillingClient.Name != null)
                    {
                        client = project.BillingClient.Name;
                    }
                    stringBuilder.AppendLine(project.Name + " with " + client);
                }    
                
                return new SlackMessageResponse(stringBuilder.ToString(), true);
            }
        }
    }
