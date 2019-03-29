using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using TimeTracker.Library.Models;

namespace TimeTracker.Library
{
    /// <summary>
    /// interprets dates in human readable form and returns them in datetime object UTC
    /// </summary>
    public static class EasyDateParser
    {
        private static readonly List<string> SupportedDateFormats = new List<string>
        {
            "yyyy-MM-d",   // ex: 2018-12-9
            "MMM-d",       // ex: jan-3
            "MMMM-d-yyyy", // ex: December-9-2017
            "MMM-d-yyyy"   // ex: Dec-9-2017
        };

        public static DateTime ParseEasyDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return GetUtcNow();
            
            var supportedValues = new Dictionary<Func<string, bool>, Func<string, DateTime>>
            {
                { x => x.Equals("yesterday", StringComparison.OrdinalIgnoreCase), x => GetUtcNow().AddDays(-1)},
                { IsSupportedDateFormat, FromSupportedDateFormat }
            };

            foreach (var supportedValue in supportedValues)
            {
                if (supportedValue.Key(date))
                    return supportedValue.Value(date);
            }

            return GetUtcNow();
        }

        public static bool IsSupportedDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            
            var supportedValues = new List<Func<string, bool>>
            {
                x => x.Equals("yesterday", StringComparison.OrdinalIgnoreCase),
                IsSupportedDateFormat
            };

            foreach (var supportedValue in supportedValues)
            {
                if (supportedValue(text))
                    return true;
            }

            return false;
        }

        private static bool IsSupportedDateFormat(string text)
        {
            return SupportedDateFormats.Any(format =>
                DateTime.TryParseExact(text, format, new CultureInfo("en-US"), DateTimeStyles.None, out _));
        }

        private static DateTime FromSupportedDateFormat(string text)
        {
            var supportedDateFormat = SupportedDateFormats.First(format => DateTime.TryParseExact(text, format, new CultureInfo("en-US"), DateTimeStyles.None, out _));
            return DateTime.ParseExact(text, supportedDateFormat, new CultureInfo("en-US"), DateTimeStyles.None);
        }

        /// <summary>
        /// Gets current UTC date but at beginning of day so as not to have TZ issues
        /// </summary>
        /// <returns></returns>
        public static DateTime GetUtcNow()
        {
            return DateTime.UtcNow.Date;
        }
    }
}