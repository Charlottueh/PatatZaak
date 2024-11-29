using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Businesslayer
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int Ordernumber { get; set; }

        ICollection<Product>? Products { get; set; }
    }
}
