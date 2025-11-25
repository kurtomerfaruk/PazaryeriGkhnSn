using System.Text.Json.Serialization;

namespace Pazaryeri.Entity.Trendyol.Response
{
    public class BatchRequestIdResponse
    {
        [JsonPropertyName("batchRequestId")]
        public string batchRequestId { get; set; }
    }
}
