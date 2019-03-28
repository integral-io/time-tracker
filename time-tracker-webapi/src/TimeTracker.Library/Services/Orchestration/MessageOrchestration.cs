using System.Threading.Tasks;
using TimeTracker.Library.Models;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library.Services.Orchestration
{
    public abstract class MessageOrchestration
    {
        public async Task<SlackMessage> GenerateResponse(SlashCommandPayload payload)
        {
            Guard.ThrowIfNull(payload);
            
            var response = await RespondTo(payload);
            
            return ToMessage(response);
        }
        protected abstract Task<SlackMessageResponse> RespondTo(SlashCommandPayload slashCommandPayload);

        private static SlackMessage ToMessage(SlackMessageResponse response)
        {
            return new SlackMessage
            {
                Text = response.MessageType == "success" ? response.Text : $"Error: *{response.Text}*"
            };
        }

        protected class SlackMessageResponse
        {
            public SlackMessageResponse(string text, string messageType)
            {
                Text = text;
                MessageType = messageType;
            }

            public string Text { get; }
            public string MessageType { get; }
        }
    }
}