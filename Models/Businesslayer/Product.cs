using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Businesslayer
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string? ProductName { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }   


    }
}
