using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker.Library
{
    /// <summary>
    /// interprets dates in human readable form and returns them in datetime object UTC
    /// </summary>
    public static class EasyDateParser
    {
        /// <summary>
        /// supports jan-21 or jan-21-2018. returns null if cant parse
        /// </summary>
        /// <param name="humanDate">supports jan-21 or jan-21-2018</param>
        /// <returns></returns>
        public static DateTime? ParseEasyDate(string humanDate)
        {
            if (string.IsNullOrWhiteSpace(humanDate))
            {
                return null;
            }
            if (humanDate == "yesterday")
            {
                return GetUtcNow().AddDays(-1);
            }
            // first check if .net parsable style date
            if (DateTime.TryParse(humanDate, out var dateTime))
            {
                return dateTime;
            }
            
            string[] split = humanDate.ToLowerInvariant().Split('-');
            
            if (split.Length >= 2)
            {
                int month = 1;
                var monthEntry = GetMonths().FirstOrDefault(x => split[0].StartsWith(x.Value));
                month = monthEntry.Key;
                int day = 0;
                if (int.TryParse(split[1], out day))
                {
                    int year = DateTime.UtcNow.Year;
                    
                    if (split.Length > 2)
                    {
                        int.TryParse(split[2], out year);
                    }

                    if (day <= 31 && month <= 12)
                    {
                        return new DateTime(year, month, day, 1, 1, 1, DateTimeKind.Utc);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets current UTC date but at beginning of day so as not to have TZ issues
        /// </summary>
        /// <returns></returns>
        public static DateTime GetUtcNow()
        {
            var utcNow = DateTime.UtcNow;
            return new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 1, DateTimeKind.Utc);
        }

        private static IReadOnlyDictionary<int, string> GetMonths()
        {
            var dict = new Dictionary<int, string>
            {
                {1, "jan"}, {2,"feb"},
                {3,"mar"},{4,"apr"},
                {5,"may"},{6,"jun"},
                {7,"jul"},{8,"aug"},
                {9,"sep"},{10,"oct"},
                {11,"nov"}, {12,"dec"}
            };

            return dict;
        }
    }
}