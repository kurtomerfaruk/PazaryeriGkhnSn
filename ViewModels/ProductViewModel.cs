using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pazaryeri.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [Display(Name = "Ürün Adı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Ürün kodu zorunludur")]
        [Display(Name = "Ürün Kodu")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage ="Ana Ürün Kodu zorunludur.")]
        [Display(Name ="Ana Ürün Kodu")]
        public string ProductMainId { get; set; }

        [Required(ErrorMessage ="Açıklama zorunludur")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage ="Fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        [Display(Name ="Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok Miktarı zorunludur")]
        [Display(Name ="Stok Miktarı")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı geçersiz")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage ="Marka seçiniz...")]
        [Display(Name = "Marka")]
        public int? BrandId { get; set; }

        [Required(ErrorMessage = "Kategori seçiniz...")]
        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }

        public List<SelectListItem> Brands { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();

        public List<IFormFile> ImageFiles { get; set; } = new();

        public List<ProductImageViewModel> ExistingImages { get; set; } = new();

        public List<ProductVariantViewModel> Variants { get; set; } = new();

        public List<CategoryAttributeViewModel> CategoryAttributes { get; set; } = new();

        public List<int> VariantIds { get; set; } = new List<int>();

        public Dictionary<int, string> AttributeValues { get; set; } = new Dictionary<int, string>();

    }
}
