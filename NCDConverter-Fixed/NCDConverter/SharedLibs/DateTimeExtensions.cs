using System;

namespace RespondClient.DomiKnow.NinjaTrader
{
    public static class DateTimeExtensions
    {
        public static bool Between(this DateTime dt, DateTime start, DateTime end)
        {
            return dt >= start && dt <= end;
        }
    }
}
