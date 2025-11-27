using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int TempId { get; set; }
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Barcode { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;


        public Product Product { get; set; }

        public List<ProductVariantImage> VariantImages { get; set; } = new();
        public virtual ICollection<ProductVariantAttribute> VariantAttributes { get; set; } = new List<ProductVariantAttribute>();
    }
}
