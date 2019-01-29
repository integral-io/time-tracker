using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker.Api
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