namespace Pazaryeri.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string TrendyolClaimId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public DateTime ClaimDate { get; set; }
        public string CargoTrackingNumber { get; set; }
        public string CargoName { get; set; }
        public string OrderShipmentPackageId { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public Platform Platform { get; set; }

        public virtual ICollection<TrendyolClaim> Trendyols { get; set; } = new List<TrendyolClaim>();
    }
}
