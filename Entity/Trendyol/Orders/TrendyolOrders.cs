namespace Pazaryeri.Entity.Trendyol.Orders
{
    public class TrendyolOrders
    {
        public int page { get; set; }
        public int size { get; set; }
        public int totalPages { get; set; }
        public int totalElements { get; set; }
        public IList<OrderContent> content { get; set; }
    }
}
