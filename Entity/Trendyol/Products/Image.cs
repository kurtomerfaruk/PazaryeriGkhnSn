using System.Text.Json.Serialization;

namespace Pazaryeri.Entity.Trendyol.Products
{
    public class Image
    {
        [JsonPropertyName("url")]
        public string url { get; set; }
    }
}
