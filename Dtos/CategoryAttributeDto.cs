using Pazaryeri.Models;

namespace Pazaryeri.Dtos
{
    public class CategoryAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool AllowCustom { get; set; }
        public bool Required { get; set; }
        public bool AllowMultipleAttributeValues { get; set; }
        public bool Varianter { get; set; }
        public List<CategoryAttributeValueDto> Values { get; set; } = new();
        public Platform Platform { get; set; }
    }
}
