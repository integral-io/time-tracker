using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker.Library.Services.Orchestration
{
    public enum SlackMessageOptions
    {
        Help = 0,
        
        [Alias("/sick", "/vacation", "/project", "/nonbill")]
        Record,
        Delete,
        Summary,
        Web,
        Projects
    }

    public class AliasAttribute : Attribute
    {
        public string[] Aliases { get; }
        
        public AliasAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }
    }

    public static class SlackMessageOptionsExtensions
    {
        public static IEnumerable<string> GetAliases(this SlackMessageOptions option)
        {
            var memberInfos = option.GetType().GetMember(option.ToString());
            var attribute = memberInfos[0].GetCustomAttributes(typeof(AliasAttribute), false);
            return ((AliasAttribute) attribute.ElementAt(0)).Aliases;
        }
    }
}