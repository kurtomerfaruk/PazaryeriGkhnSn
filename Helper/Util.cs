namespace Pazaryeri.Helper
{
    public class Util
    {
        public static DateTime LongToDatetime(long value)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
        }
    }
}
