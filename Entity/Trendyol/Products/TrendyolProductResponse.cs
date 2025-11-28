namespace Pazaryeri.Entity.Trendyol.Products
{
    public class TrendyolProductItemsResponse
    {
        public List<TrendyolProductResponse> items {  get; set; }
    }
    public class TrendyolProductResponse
    {
        public string barcode { get; set; }
        public string title { get; set; }
        public string productMainId { get; set; }
        public int brandId { get; set; }
        public int categoryId { get; set; }
        public int quantity { get; set; }
        public string stockCode { get; set; }
        public int dimensionalWeight { get; set; }
        public string description { get; set; }
        public string currencyType { get; set; } = "TRY";
        public double listPrice { get; set; }
        public double salePrice { get; set; }
        public int vatRate { get; set; } = 20;
        public int cargoCompanyId { get; set; }
        public string lotNumber { get; set; } 
        public DeliveryOption deliveryOption { get; set; }
        public List<Image> images { get; set; }
        public List<Attribute> attributes { get; set; } = new();
    }

    public class DeliveryOption
    {
        public int deliveryDuration { get; set; }
        public string fastDeliveryType { get; set; } = "FAST_DELIVERY";
    }

    public class Attribute
    {
        public int attributeId { get; set; }
        public int? attributeValueId { get; set; }
        public string? customAttributeValue { get; set; }
    }
}
