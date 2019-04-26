using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeTracker.Library.Models;
using TimeTracker.Library.Services.Orchestration;

namespace TimeTracker.Library.Services.Interpretation
{
    public enum ReportType
    {
        Default,
        ForMonth,
        ForYear,
        ForSpecificDate,
        ForMostRecent
    }
    
    public class ReportInterpretedMessage : InterpretedMessage
    {
        public string Month { get; set; }
        public string Year { get; set; }
        public bool HasDate { get; set; }
        public bool GetLastEntries { get; set; }
        public ReportType ToGenerate { get; set; }
    }

    public class ReportInterpreter : SlackMessageInterpreter<ReportInterpretedMessage>
    {
        public ReportInterpreter() : base(SlackMessageOptions.Report)
        {
        }

        public override string HelpMessage => new StringBuilder()
            .AppendLine("*/hours* report _generate default report of hours for week, month, and ytd_")
            .AppendLine("*/hours* report month <month> <optional: year> _generate report of hours for month (ie. apr) default is current year_")
            .AppendLine("*/hours* report year <year> _generate report of hours for year_")
            .AppendLine("*/hours* report date <date> _generate report for day (include dashes)_")
            .AppendLine("*/hours* report last _generate report for last ten days_")
            .ToString();

        protected override void ExtractInto(ReportInterpretedMessage message,
            List<TextMessagePart> splitText)
        {
            if (UserRequestedReportOfMostRecentTimeEntries(splitText))
            {
                message.GetLastEntries = true;
                message.ToGenerate = ReportType.ForMostRecent;
                splitText.ElementAt(1).IsUsed = true;
            }
            else if (splitText.Count > 2)
            {
                message.GetLastEntries = false;
                if (UserRequestedReportForTheMonth(splitText))
                {
                    SetUpReportForMonth(message, splitText);
                    message.ToGenerate = ReportType.ForMonth;
                }
                if (UserRequestedReportForTheYear(splitText))
                {
                    CreateDateWithYear(message, splitText);
                    message.ToGenerate = ReportType.ForYear;
                }
                if (UserRequestedReportForSpecificDate(splitText))
                {
                    splitText.ElementAt(1).IsUsed = true;
                    message.HasDate = true;
                    message.ToGenerate = ReportType.ForSpecificDate;
                }
            }
        }

        private static bool UserRequestedReportForSpecificDate(List<TextMessagePart> splitText)
        {
            return splitText.ElementAt(1).Text.Equals("date");
        }

        private static bool UserRequestedReportForTheYear(List<TextMessagePart> splitText)
        {
            return splitText.ElementAt(1).Text.Equals("year");
        }

        private static bool UserRequestedReportForTheMonth(List<TextMessagePart> splitText)
        {
            return splitText.Count > 1 && splitText.ElementAt(1).Text.Equals("month");
        }

        private static bool UserRequestedReportOfMostRecentTimeEntries(List<TextMessagePart> splitText)
        {
            return splitText.Count > 1 && splitText.ElementAt(1).Text.Equals("last");
        }

        private static void SetUpReportForMonth(ReportInterpretedMessage message, List<TextMessagePart> splitText)
        {
            splitText.ElementAt(1).IsUsed = true;
            var parseEnd = splitText.ElementAt(2).Text.Split(' ', '-');

            if (MonthYearHasDashes(parseEnd))
            {
                ParseDashesToCreateDateWithMonthYear(message, splitText, parseEnd);
            }
            else if (MonthYearDoesNotHaveDashes(splitText))
            {
                CreateDateWithMonthYear(message, splitText);
            }
            else
            {
                CreateDateWithMonthAndDefaultYear(message, splitText);
            }
        }
        
        private static void CreateDateWithMonthYear(ReportInterpretedMessage message, List<TextMessagePart> splitText)
        {
            message.Month = splitText.ElementAt(2).Text;
            splitText.ElementAt(2).IsUsed = true;
            message.Year = splitText.ElementAt(3).Text;
            splitText.ElementAt(3).IsUsed = true;
            message.Date = new DateTime(Convert.ToInt32(message.Year), message.Month.ToMonth(), 1);
        }

        private static void ParseDashesToCreateDateWithMonthYear(ReportInterpretedMessage message, List<TextMessagePart> splitText, string[] parseEnd)
        {
            message.Month = parseEnd[0];
            message.Year = parseEnd[1];
            splitText.ElementAt(2).IsUsed = true;
            message.Date = new DateTime(Convert.ToInt32(message.Year), message.Month.ToMonth(), 1);
        }

        private static void CreateDateWithMonthAndDefaultYear(ReportInterpretedMessage message, List<TextMessagePart> splitText)
        {
            message.Month = splitText.ElementAt(2).Text;
            splitText.ElementAt(2).IsUsed = true;
            message.Year = DateTime.UtcNow.Year.ToString();
            message.Date = new DateTime(DateTime.UtcNow.Year, message.Month.ToMonth(), 1);
        }

        private static void CreateDateWithYear(ReportInterpretedMessage message, List<TextMessagePart> splitText)
        {
            splitText.ElementAt(1).IsUsed = true;
            message.Year = splitText.ElementAt(2).Text;
            splitText.ElementAt(2).IsUsed = true;
            message.Date = new DateTime(Convert.ToInt32(splitText.ElementAt(2).Text), 1, 1);
        }
        
        private static bool MonthYearDoesNotHaveDashes(List<TextMessagePart> splitText)
        {
            return splitText.Count > 3;
        }

        private static bool MonthYearHasDashes(string[] parseEnd)
        {
            return parseEnd.Length == 2;
        }
    }
}