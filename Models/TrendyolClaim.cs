namespace Pazaryeri.Models
{
    public class TrendyolClaim
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string TrendyolClaimId { get; set; }
        public string Items { get; set; }

        public virtual Claim Claim { get; set; }
    }
}
