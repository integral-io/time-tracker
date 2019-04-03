using System.Threading.Tasks;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Interpretation;
using TimeTracker.Library.Utils;

namespace TimeTracker.Library.Services.Orchestration
{
    public interface IMessageOrchestration
    {
        Task<SlackMessage> GenerateResponse(SlashCommandPayload payload);
    }
    
    public abstract class MessageOrchestration<TInterpreter, TInterpretedMessage> : IMessageOrchestration
        where TInterpreter : SlackMessageInterpreter<TInterpretedMessage>, new()
        where TInterpretedMessage : InterpretedMessage, new()
    {
        public async Task<SlackMessage> GenerateResponse(SlashCommandPayload payload)
        {
            var interpretMessage = new TInterpreter().InterpretMessage(payload);

            if (interpretMessage.HasError)
            {
                return new SlackMessage
                {
                    Text = $"Error: *{interpretMessage.ErrorMessage}*"
                };
            }
            
            var response = await RespondTo(interpretMessage);

            return ToMessage(response);
        }

        protected abstract Task<SlackMessageResponse> RespondTo(TInterpretedMessage command);

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