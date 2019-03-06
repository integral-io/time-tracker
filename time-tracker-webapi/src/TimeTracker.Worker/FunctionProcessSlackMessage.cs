using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Data;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services;

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
        public static async Task Run([ServiceBusTrigger(SlackQueueName, Connection = "itt_commands_ServiceBus")]string message, ILogger logger, ExecutionContext context)
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

            var typedMessage = SlashCommandPayload.ParseFromFormEncodedData(message);

            SlackMessageOrchestrator orchestrator = _serviceProvider.GetService<SlackMessageOrchestrator>();
            var responseMessage = await orchestrator.HandleCommand(typedMessage);
            
            SlackMessageResponder slackResponder = new SlackMessageResponder(logger);
            await slackResponder.SendMessage(typedMessage.response_url, responseMessage);
        }
    }
}
