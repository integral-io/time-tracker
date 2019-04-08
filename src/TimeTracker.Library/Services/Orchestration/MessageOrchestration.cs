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
            var interpreter = new TInterpreter();
            var interpretMessage = interpreter.InterpretMessage(payload);

            if (interpretMessage.HasError)
            {
                return new SlackMessage
                {
                    Text = $"Error: *{interpretMessage.ErrorMessage}*"
                };
            }

            if (interpretMessage.IsHelp)
            {
                return new SlackMessage
                {
                    Text = interpreter.HelpMessage
                };
            }

            return (await RespondTo(interpretMessage)).ToMessage();
        }

        protected abstract Task<SlackMessageResponse> RespondTo(TInterpretedMessage message);

        protected class SlackMessageResponse
        {
            public SlackMessageResponse(string text, bool isSuccess)
            {
                Text = text;
                IsSuccess = isSuccess;
            }

            private string Text { get; }
            private bool IsSuccess { get; }

            public SlackMessage ToMessage()
            {
                return new SlackMessage
                {
                    Text = IsSuccess ? Text : $"Error: *{Text}*"
                };
            }
        }
    }
}