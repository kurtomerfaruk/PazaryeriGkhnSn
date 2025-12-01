namespace Pazaryeri.Entity.Trendyol.Claims
{
    public class TrendyolClaims
    {
        public int totalElements { get; set; }
        public int totalPages { get; set; }
        public int page { get; set; }
        public int size { get; set; }
        public List<ClaimContent> content { get; set; }
    }

    public class ClaimContent
    {
        public string id { get; set; }
        public string orderNumber { get; set; }
        public long orderDate { get; set; }
        public string customerFirstName { get; set; }
        public string customerLastName { get; set; }
        public long claimDate { get; set; }
        public long cargoTrackingNumber { get; set; }
        public string cargoProviderName { get; set; }
        public long orderShipmentPackageId { get; set; }
        public List<ClaimItems> items { get; set; }
        public long lastModifiedDate { get; set; }
        public long? orderOutboundPackageId { get; set; }
        public string cargoTrackingLink { get; set; }
    }

    public class ClaimItems
    {
        public OrderLine orderLine { get; set; }
        public List<ClaimItem> claimItems { get; set; }
    }

    public class OrderLine
    {
        public long id { get; set; }
        public string productName { get; set; }
        public string barcode { get; set; }
        public string merchantSku { get; set; }
        public string productColor { get; set; }
        public string productSize { get; set; }
        public double price { get; set; }
        public int vatBaseAmount { get; set; }
        public int salesCampaignId { get; set; }
        public string productCategory { get; set; }
        public object lineItems { get; set; }
    }

    public class ClaimItem
    {
        public string id { get; set; }
        public long orderLineItemId { get; set; }
        public ClaimItemReason customerClaimItemReason { get; set; }
        public ClaimItemReason trendyolClaimItemReason { get; set; }
        public ClaimItemStatus claimItemStatus { get; set; }
        public string note { get; set; }
        public bool resolved { get; set; }
        public object autoAccepted { get; set; }
        public bool? acceptedBySeller { get; set; }
        public long? autoApproveDate { get; set; }
        public string customerNote { get; set; }
    }

    public class ClaimItemReason
    {
        public int id { get; set; }
        public string name { get; set; }
        public int externalReasonId { get; set; }
        public string code { get; set; }
    }

    public class ClaimItemStatus
    {
        public string name { get; set; }
    }
}
