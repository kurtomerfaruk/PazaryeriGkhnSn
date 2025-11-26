namespace Pazaryeri.Entity.Trendyol.Transaction
{
    public class TransactionContent
    {
        public string id { get; set; }
        public long transactionDate { get; set; }
        public string barcode { get; set; }
        public string transactionType { get; set; }
        public string receiptId { get; set; }
        public string description { get; set; }
        public double debt { get; set; }
        public double credit { get; set; }
        public int? paymentPeriod { get; set; }
        public double? commissionRate { get; set; }
        public double? commissionAmount { get; set; }
        public string commissionInvoiceSerialNumber { get; set; }
        public double? sellerRevenue { get; set; }
        public string orderNumber { get; set; }
        public long? orderDate { get; set; }
        public int paymentOrderId { get; set; }
        public long paymentDate { get; set; }
        public int sellerId { get; set; }
        public string storeId { get; set; }
        public string storeName { get; set; }
        public string storeAddress { get; set; }
        public string country { get; set; }
        public string currency { get; set; }
        public string affiliate { get; set; }
        public string shipmentPackageId { get; set; }
    }
}
