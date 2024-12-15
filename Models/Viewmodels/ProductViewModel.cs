using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; }
        public int ProductQuantity { get; set; }
        public string? Photopath { get; set; }
        public int QuantityInCart { get; set; }
        public List<Addon> AvailableAddons { get; set; } // List of Addons for the product
    }
}