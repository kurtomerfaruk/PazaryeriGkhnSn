namespace Pazaryeri.Helper
{
    public class Util
    {
        public static DateTime LongToDatetime(long value)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
        }

        public static DateTime? LongToDatetime(long? value)
        {
            return value.HasValue ? LongToDatetime(value.Value) : (DateTime?)null;
        }

        public static long DateTimeToLong(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        public static string GetTrendyolQuestionStatus(string trendyolStatus)
        {
            return trendyolStatus switch
            {
                "WAITING_FOR_ANSWER" => "Cevap Bekleniyor",
                "WAITING_FOR_APPROVE" => "Onay Bekleniyor",
                "ANSWERED" => "Cevaplandı",
                "REPORTED" => "Bildirildi",
                "REJECTED" => "Reddedildi",
                _ => "Cevap Bekleniyor"
            };
        }
    }
}
