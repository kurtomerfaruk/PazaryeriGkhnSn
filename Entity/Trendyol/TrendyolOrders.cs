namespace Pazaryeri.Entity.Trendyol
{
    public class TrendyolOrders
    {
        public int page { get; set; }
        public int size { get; set; }
        public int totalPages { get; set; }
        public int totalElements { get; set; }
        public IList<Content> content { get; set; }
    }
}
