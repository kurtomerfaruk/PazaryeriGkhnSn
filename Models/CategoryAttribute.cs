namespace Pazaryeri.Models
{
    public class CategoryAttribute
    {
        public int Id { get; set; }
        public int CategoryAttributeId {  get; set; }   
        public string Name { get; set; }
        public bool AllowCustom { get; set; }
        public bool Required { get; set; }
        public bool Varianter { get; set; }
        public bool Slicer { get; set; }
        public bool AllowMultipleAttributeValues { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<CategoryAttributeValue> Values { get; set; } = new List<CategoryAttributeValue>();
        public Platform Platform { get; set; } = Platform.Trendyol;
    }
}
