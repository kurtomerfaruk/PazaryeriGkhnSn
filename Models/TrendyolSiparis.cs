namespace Pazaryeri.Models
{
    public class TrendyolSiparis
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string TaxNumber { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTyDiscount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
