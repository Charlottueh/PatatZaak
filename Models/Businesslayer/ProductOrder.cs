namespace PatatZaak.Models.Businesslayer
{
    public class ProductOrder
    {
        public int ProductOrderId { get; set; }  // Primary key for the join table

        public int OrderId { get; set; }  // Foreign key to Order
        public Order Order { get; set; }

        public int ProductId { get; set; }  // Foreign key to Product
        public Product Product { get; set; }
        public string ProductName { get; set; }
        public int ProductQuantity { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
