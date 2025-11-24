
namespace Pazaryeri.Models
{
    public class TrendyolOrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string ProductSize { get; set; }
        public string Sku { get; set; }
        public string MerchantSku { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public long LineId { get; set; }
    }
}
