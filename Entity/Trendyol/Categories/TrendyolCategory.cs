namespace Pazaryeri.Entity.Trendyol.Categories
{
    public class TrendyolCategory
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? parentId { get; set; }
        public List<TrendyolCategory> subCategories { get; set; }
    }
}
