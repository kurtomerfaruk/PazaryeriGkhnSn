namespace Pazaryeri.Entity.Trendyol.Orders
{
    public class Line
    {
        public int quantity { get; set; }
        public long salesCampaignId { get; set; }
        public string productSize { get; set; }
        public string merchantSku { get; set; }
        public string productName { get; set; }
        public int productCode { get; set; }
        public int merchantId { get; set; }
        public double amount { get; set; }
        public double discount { get; set; }
        public IList<DiscountDetail> discountDetails { get; set; }
        public string currencyCode { get; set; }
        public long id { get; set; }
        public string sku { get; set; }
        public double vatBaseAmount { get; set; }
        public string barcode { get; set; }
        public string orderLineItemStatusName { get; set; }
        public double price { get; set; }
    }
}
