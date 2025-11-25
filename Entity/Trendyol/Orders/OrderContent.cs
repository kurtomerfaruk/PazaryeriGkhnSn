namespace Pazaryeri.Entity.Trendyol.Orders
{
    public class OrderContent
    {
        public Address shipmentAddress { get; set; }
        public string orderNumber { get; set; }
        public double grossAmount { get; set; }
        public double totalDiscount { get; set; }
        public string taxNumber { get; set; }
        public Address invoiceAddress { get; set; }
        public string customerFirstName { get; set; }
        public string customerEmail { get; set; }
        public int customerId { get; set; }
        public string customerLastName { get; set; }
        public long id { get; set; }
        public string cargoTrackingNumber { get; set; }
        public string cargoProviderName { get; set; }
        public IList<Line> lines { get; set; }
        public long orderDate { get; set; }
        public string identityNumber { get; set; }
        public string currencyCode { get; set; }
        public IList<PackageHistory> packageHistories { get; set; }
        public string shipmentPackageStatus { get; set; }
        public string deliveryType { get; set; }
        public int timeSlotId { get; set; }
        public string scheduledDeliveryStoreId { get; set; }
        public long estimatedDeliveryStartDate { get; set; }
        public long estimatedDeliveryEndDate { get; set; }
        public double totalPrice { get; set; }
        public string cargoTrackingLink { get; set; }
        public string cargoSenderNumber { get; set; }
        public string status { get; set; }
    }
}
