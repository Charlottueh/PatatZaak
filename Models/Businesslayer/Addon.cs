using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Businesslayer
{
    public class Addon
    {
        [Key] 
        public int Identifier { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public int ProductId { get; set; }  // Foreign key to Product

        // Navigation property to Product
        public Product Product { get; set; }
        
    }
}
