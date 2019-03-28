using System.Linq;

namespace TimeTracker.Library
{
    public static class StringExtensions
    {
        public static string GetFirstWord(this string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "" : text.Split(' ').FirstOrDefault();
        }
    }
}