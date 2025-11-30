namespace Pazaryeri.Models
{
    public class ProductAttribute
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Product Product { get; set; }
    }
}
