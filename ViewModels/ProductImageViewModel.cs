namespace Pazaryeri.ViewModels
{
    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMainImage { get; set; }
        public int SortOrder { get; set; }
    }
}
