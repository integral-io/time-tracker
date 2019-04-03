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
            sb.AppendLine("*/hours* record <projectName> <hours> _Will use today date by default_");
            sb.AppendLine("*/hours* record <projectName> <hours> jan-21 _sets date to january 21 current year_");
            sb.AppendLine("*/hours* record <projectName> <hours> wfh _wfh option marks as worked from home_");
            sb.AppendLine("*/hours* record nonbill <hours> <optional: date> \"non billable reason\" _non billable hour for a given reason, ie PDA_");
            sb.AppendLine("*/hours* record sick <hours> <optional: date> _marks sick hours_");
            sb.AppendLine("*/hours* record vacation <hours> <optional: date> _marks vacation hours_");
            sb.AppendLine("*/hours* report <optional: date> _generate report of hours_");
            sb.AppendLine("*/hours* delete <optional: date> _delete all hours for the date_");

            return Task.FromResult(new SlackMessage
            {
                Text = sb.ToString()
            });
        }
    }
}