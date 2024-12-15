using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class CartViewModel
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartProductViewModel> Products { get; set; }
    }

    public class CartProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
    }
}