namespace Pazaryeri.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CategoryAttributeDto> CategoryAttributes { get; set; } = new();
    }
}
