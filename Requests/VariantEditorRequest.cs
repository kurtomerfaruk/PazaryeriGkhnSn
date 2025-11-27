using Pazaryeri.ViewModels;

namespace Pazaryeri.Requests
{
    public class VariantEditorRequest
    {
        public VariantEditorRequest()
        {
            VariationAttributes = new List<CategoryAttributeViewModel>();
            VariantData = new ProductVariantViewModel();
        }

        public int TempId { get; set; }
        public List<CategoryAttributeViewModel> VariationAttributes { get; set; }
        public ProductVariantViewModel VariantData { get; set; }
    }
}
