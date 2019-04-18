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

        public bool HasDate { get; set; }
    }

    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedMessage>
    {
        public ReportInterpreter() : base("report")
        {
        }

        public override string HelpMessage => new StringBuilder()
            .AppendLine("*/hours* report _generate default report of hours for month and ytd_")
            .AppendLine("*/hours* report month <month> <optional: year> _generate report of hours for month (ie. apr) default is current year_")
            .AppendLine("*/hours* report year <year> _generate report of hours for year_")
            .AppendLine("*/hours* report date <date> _generate report for day (include dashes)_")
            .ToString();

        protected override void ExtractInto(ReportInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
            if (splitText.Count > 2)
            {
                if (splitText.ElementAt(1).Text.Equals("month"))
                {
                    splitText.ElementAt(1).IsUsed = true;
                    var parseEnd = splitText.ElementAt(2).Text.Split(' ','-');

                    if (parseEnd.Length == 2)
                    {
                        message.Month = parseEnd[0];
                        message.Year = parseEnd[1];
                        splitText.ElementAt(2).IsUsed = true;
                        message.Date = new DateTime(Convert.ToInt32(message.Year), message.Month.ToMonth(), 1);
                    } else if (splitText.Count > 3)
                    {
                        message.Month = splitText.ElementAt(2).Text;
                        splitText.ElementAt(2).IsUsed = true;
                        message.Year  = splitText.ElementAt(3).Text;
                        splitText.ElementAt(3).IsUsed = true;
                        message.Date = new DateTime(Convert.ToInt32(message.Year), message.Month.ToMonth(), 1);
                    }
                    else
                    {
                        message.Month = splitText.ElementAt(2).Text;
                        splitText.ElementAt(2).IsUsed = true;
                        message.Year = DateTime.UtcNow.Year.ToString();
                        message.Date = new DateTime(DateTime.UtcNow.Year, message.Month.ToMonth(), 1);
                    }
                }
                
                if (splitText.ElementAt(1).Text.Equals("year"))
                {
                    splitText.ElementAt(1).IsUsed = true;
                    message.Year = splitText.ElementAt(2).Text;
                    splitText.ElementAt(2).IsUsed = true;
                    message.Date = new DateTime(Convert.ToInt32(splitText.ElementAt(2).Text), 1, 1);
                }

                if (splitText.ElementAt(1).Text.Equals("date"))
                {
                    splitText.ElementAt(1).IsUsed = true;
                    message.HasDate = true;
                }
            }
        }
    }

    public static class MonthConverter
    {
        public static int ToMonth(this String month)
        {
            month = month.ToLower();
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