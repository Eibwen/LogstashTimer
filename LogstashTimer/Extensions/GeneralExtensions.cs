using System;

namespace LogstashTimer.Extensions
{
    public static class GeneralExtensions
    {
        public static TimeSpan? GetTimespan(this TimeSpan time)
        {
            if (time.TotalSeconds < 1)
                return null;
            return time;
        }

        public static T IsNullOrDefault<T>(this T obj, T valueToReturnIfSo)
        {
            return (obj == null
                    || obj.Equals(default(T)))
                       ? valueToReturnIfSo
                       : obj;
        }
    }
}