using System;
using System.Linq;

namespace TimeTracker.Api
{
    public static class SlackMessageInterpreter
    {
        public const string OPTION_RECORD = "record";
        public const string OPTION_REPORT = "report";
        
        /// <summary>
        /// parse command string and return typed object
        /// </summary>
        /// <param name="text">ex: record au 8 wfh. or record au 8 yesterday.</param>
        /// <returns></returns>
        public static HoursInterpretedCommandDto InterpretHoursRecordMessage(string text)
        {
            string[] splitText = text.ToLowerInvariant().Split(' ');
            if (!text.StartsWith("record"))
            {
                return new HoursInterpretedCommandDto()
                    {ErrorMessage = $"Invalid start option: {splitText.FirstOrDefault()}"};
            }
            
            var dto = new HoursInterpretedCommandDto();
            dto.Project = splitText[1];
            dto.IsWorkFromHome = text.Contains("wfh");
            dto.Hours = Convert.ToDouble(splitText[2]);
            dto.IsBillable = true;
            // process date portion
            string datePortion = splitText.Length >= 4 ? splitText[3] : null;
            DateTime dateTime = DateTime.UtcNow;
            if (datePortion == "yesterday")
            {
                dto.Date = dateTime.AddDays(-1);
            }
            else if (DateTime.TryParse(datePortion, out dateTime))
            {
                dto.Date = dateTime;
            }
            else
            {
                dto.Date = dateTime;
            }
            
            return dto;
        }
    }

    public class HoursInterpretedCommandDto
    {
        public string Project { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public bool IsWorkFromHome { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsBillable { get; set; }
        public string NonBillReason { get; set; }
    }
}