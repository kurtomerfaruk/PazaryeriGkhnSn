using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "Varyasyon tipi zorunludur")]
        //[Display(Name = "Varyasyon Tipi")]
        //public string AttributeType { get; set; }

        //[Required(ErrorMessage = "Varyasyon değeri zorunludur")]
        //[Display(Name = "Varyasyon Değeri")]
        //public string AttributeValue { get; set; }

        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Stok")]
        public int StockQuantity { get; set; }

        [Display(Name = "Barkod")]
        public string Barcode { get; set; }

        [Display(Name = "Varyasyon Görselleri")]
        public List<IFormFile> ImageFiles { get; set; } = new();
        public List<string> ExistingImages { get; set; } = new();
    }
}
