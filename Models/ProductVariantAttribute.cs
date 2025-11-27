namespace Pazaryeri.Models
{
    public class ProductVariantAttribute
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ProductVariant Variant { get; set; }
    }
}
