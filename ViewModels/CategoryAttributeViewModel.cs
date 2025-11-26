namespace Pazaryeri.ViewModels
{
    public class CategoryAttributeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Varianter { get; set; }
        public bool AllowCustom { get; set; }
        public bool Required { get; set; }
        public bool AllowMultipleAttributeValues { get; set; }
        public List<CategoryAttributeValueViewModel> Values { get; set; } = new();
    }
}
