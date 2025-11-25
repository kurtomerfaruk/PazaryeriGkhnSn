using Pazaryeri.Entity.Trendyol.Orders;

namespace Pazaryeri.Entity.Trendyol.Products
{
    public class TrendyolProducts
    {
        public int page { get; set; }
        public int size { get; set; }
        public int totalPages { get; set; }
        public int totalElements { get; set; }
        public IList<ProductContent> content { get; set; }
    }
}
