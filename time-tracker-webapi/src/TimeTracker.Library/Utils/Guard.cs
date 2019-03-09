using System;

namespace TimeTracker.Library.Utils
{
    public static class Guard
    {
        public static void ThrowIfNull(object argumentValue)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(nameof(argumentValue));
            }
        }

        public static void ThrowIfCheckFails(bool isValid, string reason, string paramName)
        {
            if (!isValid)
            {
                throw new ArgumentException(reason, paramName);
            }
        }
    }
}