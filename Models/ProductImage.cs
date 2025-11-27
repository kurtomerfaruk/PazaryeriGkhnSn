using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string ImageUrl { get; set; }
        public bool IsMainImage { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
