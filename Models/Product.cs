namespace Pazaryeri.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public Platform Platform { get; set; }
        public string ProductMainId { get; set; }
        public List<TrendyolProductDetail> TrendyolDetails { get; set; } = new List<TrendyolProductDetail>();
    }
}
