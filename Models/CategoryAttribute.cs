namespace Pazaryeri.Models
{
    public class CategoryAttribute
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string AttributeId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValueId { get; set; }
        public string AttributeValueName { get; set; }
        public Platform Platform { get; set; } = Platform.Trendyol;
    }
}
