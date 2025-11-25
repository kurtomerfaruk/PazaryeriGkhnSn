namespace Pazaryeri.Entity.Trendyol.Products
{
    public class ProductContent
    {
        public bool approved { get; set; }
        public bool archived { get; set; }
        public List<Attribute> attributes { get; set; }
        public string barcode { get; set; }
        public string brand { get; set; }
        public int brandId { get; set; }
        public string categoryName { get; set; }
        public long createDateTime { get; set; }
        public string description { get; set; }
        public int dimensionalWeight { get; set; }
        public bool hasActiveCampaign { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public long lastUpdateDate { get; set; }
        public double listPrice { get; set; }
        public bool locked { get; set; }
        public bool onSale { get; set; }
        public int pimCategoryId { get; set; }
        public string platformListingId { get; set; }
        public int productCode { get; set; }
        public int productContentId { get; set; }
        public string productMainId { get; set; }
        public int quantity { get; set; }
        public double salePrice { get; set; }
        public string stockCode { get; set; }
        public string stockUnitType { get; set; }
        public int supplierId { get; set; }
        public string title { get; set; }
        public int vatRate { get; set; }
        public bool rejected { get; set; }
        public List<object> rejectReasonDetails { get; set; }
        public bool blacklisted { get; set; }
        public bool hasHtmlContent { get; set; }
        public string productUrl { get; set; }
        public object deliveryOption { get; set; }
    }
}
