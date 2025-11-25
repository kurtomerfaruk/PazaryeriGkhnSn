using System.ComponentModel.DataAnnotations;

namespace Pazaryeri.Models
{
    public class TrendyolAttribute
    {
        public int Id { get; set; }

        public int AttributeId { get; set; } 

        public string AttributeName { get; set; } 

        public string AttributeValue { get; set; } 

        public int? AttributeValueId { get; set; } 

        public string ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
