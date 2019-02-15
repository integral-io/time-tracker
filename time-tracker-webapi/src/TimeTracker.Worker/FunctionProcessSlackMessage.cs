using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;

namespace TimeTracker.Worker
{
    public static class FunctionProcessSlackMessage
    {
        [FunctionName("processSlackMessage")]
        public static void Run([ServiceBusTrigger("slack-slash-commands", Connection = "itt-commands-ServiceBus")]string message, ILogger logger)
        {
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            var typedMessage = SlashCommandPayload.ParseFromFormEncodedData(message);
            // todo: process the message much like we do in controller.
            
            // improve dependency injection? what is best practice with serverless...
            SlackMessageResponder slackResponder = new SlackMessageResponder(logger);
            slackResponder.SendMessage(typedMessage.response_url, new SlackMessage()
            {
                Text = "Cool I got your message"
            });
        }
    }
}
