using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.WebUtilities;

namespace TimeTracker.Library.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SlashCommandPayload
    {
        public string token { get; set; }
        public string team_id { get; set; }
        public string team_domain { get; set; }
        public string enterprise_id { get; set; }
        public string enterprise_name { get; set; }
        public string channel_id { get; set; }
        public string channel_name { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string command { get; set; }
        public string text { get; set; }
        public string response_url { get; set; }
        public string trigger_id { get; set; }
        
        public static SlashCommandPayload ParseFromFormEncodedData(string formData)
        {
            var reader = new FormReader(formData);
            
            var commandPayload = new SlashCommandPayload();

            var keepReading = true;
            do
            {
                var nextPair = reader.ReadNextPair();
                if (nextPair.HasValue)
                {
                    var propertyInfo = typeof(SlashCommandPayload).GetProperty(nextPair.Value.Key);
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(commandPayload, nextPair.Value.Value);
                    }
                }
                else
                {
                    keepReading = false;
                }
                
            } while (keepReading);

            return commandPayload;
        }
    }
}