namespace Pazaryeri.Entity.Trendyol.Response
{
    public class StockPriceBatchResponse
    {
        public string batchRequestId { get; set; }
        public List<StockPriceItem> items { get; set; }
        public long creationDate { get; set; }
        public long lastModification { get; set; }
        public string sourceType { get; set; }
        public int itemCount { get; set; }
        public int failedItemCount { get; set; }
        public string batchRequestType { get; set; }
    }
}
