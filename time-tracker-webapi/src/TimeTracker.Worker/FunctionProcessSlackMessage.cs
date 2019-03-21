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
        private static IConfigurationRoot _configuration;
        private static ServiceProvider _serviceProvider;

        private const string SlackQueueName = "slack-slash-commands";


        private static void SetupConfiguration(ExecutionContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        private static void SetupServiceCollection()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // add db context with contable connection string
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddDbContextPool<TimeTrackerDbContext>(options =>
                {
                    options.UseSqlServer(connectionString)
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                })
                .AddScoped<SlackMessageOrchestrator>()
                .BuildServiceProvider();

            _serviceProvider = serviceProvider;
        }

        [FunctionName("processSlackMessage")]
        public static async Task Run([ServiceBusTrigger(SlackQueueName, Connection = "itt_commands_ServiceBus")]
            string message, ILogger logger, ExecutionContext context)
        {
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
            
            if (_configuration == null)
            {
                SetupConfiguration(context);
            }
            if (_serviceProvider == null)
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
                
                SlackMessageOrchestrator orchestrator = _serviceProvider.GetService<SlackMessageOrchestrator>();
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
            
            if (_configuration == null)
            {
                SetupConfiguration(context);
                Console.WriteLine("I am a jabroni who never learned how to read.");
            }
            if (_serviceProvider == null)
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
                
                SlackMessageOrchestrator orchestrator = _serviceProvider.GetService<SlackMessageOrchestrator>();
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

                throw;
            }

            return new OkResult();
        }
    }
}