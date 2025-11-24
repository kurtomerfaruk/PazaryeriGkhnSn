using DocumentFormat.OpenXml.Vml;

namespace Pazaryeri.Entity.Trendyol
{
    public class Content
    {
        public Address shipmentAddress { get; set; }
        public string orderNumber { get; set; }
        public double grossAmount { get; set; }
        public double totalDiscount { get; set; }
        public object taxNumber { get; set; }
        public Address invoiceAddress { get; set; }
        public string customerFirstName { get; set; }
        public string customerEmail { get; set; }
        public int customerId { get; set; }
        public string customerLastName { get; set; }
        public int id { get; set; }
        public object cargoTrackingNumber { get; set; }
        public string cargoProviderName { get; set; }
        public IList<Line> lines { get; set; }
        public object orderDate { get; set; }
        public string tcIdentityNumber { get; set; }
        public string currencyCode { get; set; }
        public IList<PackageHistory> packageHistories { get; set; }
        public string shipmentPackageStatus { get; set; }
        public string deliveryType { get; set; }
        public int timeSlotId { get; set; }
        public string scheduledDeliveryStoreId { get; set; }
        public object estimatedDeliveryStartDate { get; set; }
        public object estimatedDeliveryEndDate { get; set; }
        public double totalPrice { get; set; }
        public string cargoTrackingLink { get; set; }
        public string cargoSenderNumber { get; set; }
    }
}
