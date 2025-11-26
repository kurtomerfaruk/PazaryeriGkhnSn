using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class ProductVariantImage
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        [Required]
        public string ImageUrl { get; set; }
    }
}
