using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class ProductVariantImage
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
       

        [Required]
        public string ImageUrl { get; set; }
        public string TrendyolImageUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ProductVariant ProductVariant { get; set; }
    }
}
