using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Businesslayer
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int Ordernumber { get; set; }

        public int OrderStatus { get; set; }

        public ICollection<Product>? Products { get; set; }

        [Required]
        public User? OrderUser { get; set; }

        [Required]
        public int UserId { get; set; }

        public decimal TotalPrice
        {
            get => Products?.Sum(p => p.ProductPrice * p.ProductQuantity) ?? 0;
        }
    }
}
