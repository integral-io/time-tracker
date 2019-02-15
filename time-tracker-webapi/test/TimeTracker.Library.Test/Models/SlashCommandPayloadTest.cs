using System.Net;
using FluentAssertions;
using TimeTracker.Library.Models;
using Xunit;

namespace TimeTracker.Library.Test.Models
{
    public class SlashCommandPayloadTest
    {
        [Fact]
        public void ParseFromFormEncodedData_returnsCorrectValuesFromFormData()
        {
            string data = "token=gIkuvaNzQIHg97ATvDxqgjtO&team_id=T0001" +
                          "&team_domain=example&enterprise_id=E0001" +
                          "&enterprise_name=Globular%20Construct%20Inc" +
                          "&channel_id=C2147483705&channel_name=test" +
                          "&user_id=U2147483697" +
                          "&user_name=Steve" +
                          "&command=/weather" +
                          $"&text={WebUtility.UrlEncode("report acme 8 yesterday")}" +
                          $"&response_url={WebUtility.UrlEncode("https://hooks.slack.com/commands/1234/5678")}" +
                          "&trigger_id=13345224609.738474920.8088930838d88f008e0";
            
            
            var typedCommandPayload = SlashCommandPayload.ParseFromFormEncodedData(data);

            typedCommandPayload.text.Should().Be("report acme 8 yesterday");
            typedCommandPayload.response_url.Should().Be("https://hooks.slack.com/commands/1234/5678");
        }
    }
}