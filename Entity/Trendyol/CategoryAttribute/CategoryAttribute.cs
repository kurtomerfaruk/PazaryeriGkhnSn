namespace Pazaryeri.Entity.Trendyol.CategoryAttribute
{
    public class CategoryAttribute
    {
        public bool allowCustom { get; set; }
        public AttributeValue attribute { get; set; }
        public List<AttributeValue> attributeValues { get; set; }
        public int categoryId { get; set; }
        public bool required { get; set; }
        public bool varianter { get; set; }
        public bool slicer { get; set; }
        public bool allowMultipleAttributeValues { get; set; }
    }
}
