namespace Pazaryeri.Models
{
    public class CategoryAttributeValue
    {
        public int Id { get; set; } 
        public int CategoryAttributeValueId {  get; set; }
        public string Name { get; set; }
        public int CategoryAttributeId { get; set; }
        public CategoryAttribute CategoryAttribute { get; set; }
    }
}
