namespace Pazaryeri.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string Title { get; set; }
        public Platform Platform { get; set; } = Platform.Trendyol;
        public string ParentCategoryId { get; set; }
        public bool TopCategory { get; set; } = false;
    }
}
