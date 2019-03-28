using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services
{
    /// <summary>
    /// sends message to slack using the provided response_url
    /// </summary>
    public class SlackMessageResponder
    {
        private readonly ILogger logger;

        public SlackMessageResponder(in ILogger logger)
        {
            this.logger = logger;
        }

        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() => new HttpClient());

        public async Task SendMessage(string responseUrl, SlackMessage slackMessage)
        {
            var postAsync = await LazyHttpClient.Value.PostAsync(responseUrl, new JsonContent(slackMessage));
            if (!postAsync.IsSuccessStatusCode)
            {
                var errorContent = await postAsync.Content.ReadAsStringAsync();
                logger.LogError("Could not post message back to slack: " + errorContent);
            }
        }
    }

    public class JsonContent : StringContent
    {
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver() // this may need to be snake case
        };
       
        
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj, serializerSettings), Encoding.UTF8, "application/json")
        {
        }
    }
}