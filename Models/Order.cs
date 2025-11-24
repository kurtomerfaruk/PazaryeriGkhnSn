using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class Order
    {
        
        public int Id { get; set; }

        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string BillName { get; set; }
        public string BillPhone { get; set; }
        public string BillAddress { get; set; }
        public string BillDistrict { get; set; }
        public string BillCity { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Status { get; set; } = "Bekliyor";
        public OrderPlatform Platform { get; set; } = OrderPlatform.Trendyol;
        public decimal GrossAmount{ get; set; }
        public decimal TotalDiscount{ get; set; }
        public decimal TotalPrice{ get; set; }
        public List<TrendyolOrderDetail> TrendyolDetails { get; set; } = new List<TrendyolOrderDetail>();
    }

    public enum OrderPlatform
    {
        Trendyol = 0
    }

}
