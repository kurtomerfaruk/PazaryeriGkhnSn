namespace Pazaryeri.Models
{
    public class ProductVariantAttribute
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }

        // Navigation properties
        public virtual ProductVariant Variant { get; set; }
    }
}
