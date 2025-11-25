namespace Pazaryeri.Models
{
    public class TrendyolRejectReasonDetail
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
