namespace Pazaryeri.Entity.Trendyol.Transaction
{
    public class TrendyolTransactions
    {
        public int page { get; set; }
        public int size { get; set; }
        public int totalPages { get; set; }
        public int totalElements { get; set; }
        public List<TransactionContent> content { get; set; }
    }
}
