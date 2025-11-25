namespace Pazaryeri.Entity.Trendyol.CategoryAttribute
{
    public class TrendyolCategoryAttributes
    {
        public int id { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public List<CategoryAttribute> categoryAttributes { get; set; }
    }
}
