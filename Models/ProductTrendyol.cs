namespace Pazaryeri.Models
{
    public class ProductTrendyol
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string TrendyolProductId { get; set; }
        public bool IsApproved {  get; set; }   
        public bool IsOnSale { get; set; }
        public string ProductUrl { get; set; }
        public string BatchRequestId { get; set; }

        public virtual Product Product { get; set; }
    }
}
