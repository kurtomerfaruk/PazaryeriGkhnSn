using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int AttributeId { get; set; }

        [Required]
        public string AttributeValueId { get; set; }

        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Barcode { get; set; }

        public List<ProductVariantImage> VariantImages { get; set; } = new();
    }
}
