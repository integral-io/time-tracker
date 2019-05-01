using System.Text;
using System.Threading.Tasks;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Orchestration
{
    public class HelpMessageOrchestration : IMessageOrchestration
    {

        public Task<SlackMessage> GenerateResponse(SlashCommandPayload payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("*/hours* record - Allows the user to record hours.");
            sb.AppendLine("*/hours* summary - Shows the users reported hours.");
            sb.AppendLine("*/hours* delete - Deletes hours reported.");
            sb.AppendLine("*/hours* projects - Lists available projects (project - client).");
            sb.AppendLine("*/hours* web - Links user to web for hours report.");
            sb.AppendLine("Add 'help' to each one of the options to get specific help. ex: */hours* record help");

            return Task.FromResult(new SlackMessage
            {
                Text = sb.ToString()
            });
        }
    }
}