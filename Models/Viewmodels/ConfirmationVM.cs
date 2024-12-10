using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class ConfirmationVM
    {
       public int OrderNumber { get; set; }
       public string? PickupNumber { get; set; }
       public string? PickupTime { get; set; }
       public decimal TotalPrice { get; set; }
       public ICollection<Product>? Products { get; set; }
      
    }
}
