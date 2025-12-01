using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }

        public int TempId { get; set; }

        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Display(Name = "Liste Fiyatı")]
        public decimal ListPrice { get; set; }

        [Display(Name = "Satış Fiyatı")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Stok")]
        public int StockQuantity { get; set; }

        [Display(Name = "Barkod")]
        public string Barcode { get; set; }

        [Display(Name = "Varyasyon Görselleri")]
        public List<IFormFile> ImageFiles { get; set; } = new();
        public List<string> ExistingImages { get; set; } = new();

        public Dictionary<int, int> VariationAttributes { get; set; } = new Dictionary<int, int>();
    }
}
