using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;
using TimeTracker.Library.Services.Orchestration;
using TimeTracker.Library.Utils;

namespace TimeTracker.Worker
{
    public static class FunctionProcessSlackMessage
    {
        private const string SlackQueueName = "slack-slash-commands";

        private static IConfigurationRoot configuration;
        private static ServiceProvider serviceProvider;

        private static void SetupConfiguration(ExecutionContext context)
        {
            if (configuration != null) return;
            
            var builder = new ConfigurationBuilder().SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            configuration = builder.Build();
        }

        private static void SetupServiceCollection()
        {
            if (serviceProvider != null) return;
            
            serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddDbContextPool<TimeTrackerDbContext>(options =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    options.UseSqlServer(connectionString)
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                })
                .AddScoped<SlackMessageOrchestrator>()
                .BuildServiceProvider();
        }

        [FunctionName("processSlackMessageHttp")]
        public static async Task<IActionResult> RunHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest request, ILogger logger, ExecutionContext context)
        {
            logger.LogInformation($"C# Http trigger function processed message: {request}");

            SetupConfiguration(context);
            SetupServiceCollection();

            var slackResponder = new SlackMessageResponder(logger);
            string responseUrl = null;
            // todo: play with dc scope / disposing
            try
            {
                var requestBody = await new StreamReader(request.Body).ReadToEndAsync();

                var typedMessage = SlashCommandPayload.ParseFromFormEncodedData(requestBody);
                responseUrl = typedMessage.response_url;

                var orchestrator = serviceProvider.GetService<SlackMessageOrchestrator>();
                var responseMessage = await orchestrator.HandleCommand(typedMessage);

                await slackResponder.SendMessage(typedMessage.response_url, responseMessage);
            }
            catch (Exception exc)
            {
                logger.LogError(exc.Message);

                if (responseUrl != null)
                {
                    await slackResponder.SendMessage(responseUrl, new SlackMessage()
                    {
                        Text = $"*Error:* _{exc.Message}_\n Source: {exc.Source} \n {exc.StackTrace}"
                    });
                }

                return new BadRequestObjectResult(exc.Message);
            }

            return new OkResult();
        }
    }
}