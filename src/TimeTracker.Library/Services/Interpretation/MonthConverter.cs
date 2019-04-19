using System;

namespace TimeTracker.Library.Services.Interpretation
{
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