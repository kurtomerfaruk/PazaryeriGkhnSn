using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string ProductCode { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public string ProductMainId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<ProductImage> Images { get; set; } = new();
        public List<ProductVariant> Variants { get; set; } = new();

        public string TrendyolProductId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
