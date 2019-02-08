using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TimeTracker.Worker
{
    public static class ProcessSlackMessage
    {
        [FunctionName("processSlackMessage")]
        public static void Run([ServiceBusTrigger("slack-slash-commands", Connection = "itt-commands-ServiceBus")]string message, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
        }
    }
}
