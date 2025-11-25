
using System.ComponentModel.DataAnnotations.Schema;

namespace Pazaryeri.Models
{
    public class TrendyolProductDetail
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string TrenyolProductId { get; set; }
        public string ProductMainId { get; set; }
        public Product Product { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public string StockCode { get; set; }
        public double DimensionalWeight { get; set; }
        public string CurrencyType { get; set; } = "TRY";
        public decimal ListPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int VatRate { get; set; } = 20;
        public int CargoCompanyId { get; set; }
        public int ShipmentAddressId { get; set; }
        public int ReturningAddressId { get; set; }
        public long ProductCode { get; set; }
        public string ProductUrl { get; set; }
        public bool SaleStatus { get; set; } = false;
        public bool ApprovalStatus { get; set; } = false;

        [NotMapped]
        public string Title { get; set; }
        [NotMapped]
        public string Subtitle { get; set; }
        [NotMapped]
        public string Description { get; set; }

        public virtual ICollection<TrendyolAttribute> Attributes { get; set; }
        public virtual ICollection<TrendyolImage> Images { get; set; }
        public virtual ICollection<TrendyolRejectReasonDetail> RejectReasonDetails { get; set; }
    }
}
