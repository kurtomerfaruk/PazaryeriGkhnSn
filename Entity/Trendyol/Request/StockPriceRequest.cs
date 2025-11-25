using System.Text.Json.Serialization;

namespace Pazaryeri.Entity.Trendyol.Request
{
    public class StockPriceRequest
    {
        [JsonPropertyName("barcode")]
        public string barcode { get; set; }

        [JsonPropertyName("quantity")]
        public int quantity { get; set; }

        [JsonPropertyName("salePrice")]
        public decimal salePrice { get; set; }

        [JsonPropertyName("listPrice")]
        public decimal listPrice { get; set; }
    }
}
