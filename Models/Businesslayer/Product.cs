using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PatatZaak.Models.Businesslayer
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string? ProductName { get; set; }

        [Required]
        public string? ProductCat {  get; set; }

        [Required]
        public string? ProductDescription { get; set; }

        [Required]
        public int ProductPoints { get; set; }

        public bool? ProductDiscount { get; set; }

        public string? Photopath { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }   


    }
}
