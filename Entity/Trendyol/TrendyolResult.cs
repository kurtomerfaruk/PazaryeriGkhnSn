using Pazaryeri.Entity.Trendyol.Response;

namespace Pazaryeri.Entity.Trendyol
{
    public class TrendyolResult
    {
        public bool IsSuccess { get; set; }
        public TrendyolSuccessResponse Success { get; set; }
        public TrendyolErrorResponse Error { get; set; }

        // Parse edilemeyen durumlar için
        public string RawResponse { get; set; }
    }

    public class TrendyolSuccessResponse
    {
        public string batchRequestId { get; set; }
        public string status { get; set; }
    }

    public class TrendyolErrorResponse
    {
        public long timestamp { get; set; }
        public string exception { get; set; }
        public List<TrendyolErrorDetail> errors { get; set; }
    }

    public class TrendyolErrorDetail
    {
        public string key { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
    }
}
