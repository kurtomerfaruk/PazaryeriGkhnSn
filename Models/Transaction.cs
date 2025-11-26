namespace Pazaryeri.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Barcode { get; set; }
        public string TransactionType { get; set; }
        public string ReceiptId { get; set; }
        public string Description { get; set; }
        public decimal Debt { get; set; }
        public decimal Credit { get; set; }
        public int? PaymentPeriod { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal CommissionAmount { get; set; }
        public string CommissionInvoiceSerialNumber { get; set; }
        public decimal SellerRevenue { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate{ get; set; }
        public long PaymentOrderId { get; set; }
        public DateTime PaymentDate { get; set; }
        public int SellerId { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string Country { get; set; }
        public string Currency{ get; set; }
        public string Affiliate { get; set; }
        public string ShipmentPackageId { get; set; }
        public Platform Platform { get; set; }
    }
}
