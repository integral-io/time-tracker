using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
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
            var builder = new ConfigurationBuilder().SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            configuration = builder.Build();
        }

        private static void SetupServiceCollection()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddDbContextPool<TimeTrackerDbContext>(options =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    
                    options.UseSqlServer(connectionString)
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                })
                .AddScoped<SlackMessageOrchestrator>()
                .BuildServiceProvider();

            FunctionProcessSlackMessage.serviceProvider = serviceProvider;
        }

        [FunctionName("processSlackMessage")]
        public static async Task Run([ServiceBusTrigger(SlackQueueName, Connection = "itt_commands_ServiceBus")]
            string message, ILogger logger, ExecutionContext context)
        {
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
            
            if (configuration == null)
            {
                SetupConfiguration(context);
            }
            if (serviceProvider == null)
            {
                SetupServiceCollection();
            }
            
            SlackMessageResponder slackResponder = new SlackMessageResponder(logger);
            string responseUrl = null;
            
            try
            {
                Guard.ThrowIfCheckFails(!String.IsNullOrEmpty(message), "cannot be null or empty", nameof(message));
                
                var typedMessage = SlashCommandPayload.ParseFromFormEncodedData(message);
                responseUrl = typedMessage.response_url;
                
                SlackMessageOrchestrator orchestrator = serviceProvider.GetService<SlackMessageOrchestrator>();
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

                throw;
            }
        }
        
        [FunctionName("processSlackMessageHttp")]
        public static async Task<IActionResult> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest request, ILogger logger, ExecutionContext context)
        {
            logger.LogInformation($"C# Http trigger function processed message: {request}");
            
            if (configuration == null)
            {
                SetupConfiguration(context);
                Console.WriteLine("I am a jabroni who never learned how to read.");
            }
            if (serviceProvider == null)
            {
                SetupServiceCollection();
            }
            
            SlackMessageResponder slackResponder = new SlackMessageResponder(logger);
            string responseUrl = null;
            // todo: play with dc scope / disposing
            try
            {
                string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
                
                Guard.ThrowIfCheckFails(!String.IsNullOrEmpty(requestBody), "cannot be null or empty", nameof(request));
                
                var typedMessage = SlashCommandPayload.ParseFromFormEncodedData(requestBody);
                responseUrl = typedMessage.response_url;
                
                SlackMessageOrchestrator orchestrator = serviceProvider.GetService<SlackMessageOrchestrator>();
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