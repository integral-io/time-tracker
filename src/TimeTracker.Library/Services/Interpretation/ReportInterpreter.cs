using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Library.Models;

namespace TimeTracker.Library.Services.Interpretation
{
    public class ReportInterpretedMessage : InterpretedMessage
    {
        public string Month { get; set; }
        public string Year { get; set; }
    }

    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedMessage>
    {
        public ReportInterpreter() : base("report")
        {
        }

        public override string HelpMessage => new StringBuilder()
            .AppendLine("*/hours* report <optional: date> _generate report of hours for monthly and ytd_")
            .AppendLine("*/hours* report month <month> _generate report of hours for month_")
            .AppendLine("*/hours* report year <year> _generate report of hours for year_")
            .ToString();

        protected override void ExtractInto(ReportInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
            if (splitText.Count > 1)
            {
                if (splitText.ElementAt(1).Text.Equals("month"))
                {
                    splitText.ElementAt(1).IsUsed = true;
                    message.Month  = splitText.ElementAt(2).Text;
                    splitText.ElementAt(2).IsUsed = true;
                    message.Date = new DateTime(DateTime.UtcNow.Year, message.Month.ToMonth(), 1);
                }
            }
        }
    }

    public static class MonthConverter
    {
        public static int ToMonth(this String month)
        {
            switch (month)
            {
                case "jan":
                    return 1;
                case "feb":
                    return 2;
                case "mar":
                    return 3;
                case "apr":
                    return 4;
                case "may":
                    return 5;
                case "jun":
                    return 6;
                case "july":
                    return 7;
                case "aug":
                    return 8;
                case "sep":
                    return 9;
                case "sept":
                    return 9;
                case "oct":
                    return 10;
                case "nov":
                    return 11;
                case "dec":
                    return 12;
                default:
                    throw new Exception("Improper month abbreviation");
            }
        }
    }
}