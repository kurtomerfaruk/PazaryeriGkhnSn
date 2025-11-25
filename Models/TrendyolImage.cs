namespace Pazaryeri.Models
{
    public class TrendyolImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
