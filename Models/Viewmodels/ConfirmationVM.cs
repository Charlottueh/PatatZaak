using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class ConfirmationVM
    {
        public int OrderNumber { get; set; }
       
        public DateTime PickupTime { get; set; }
        public decimal TotalPrice { get; set; }
        public List<ProductVM> Products { get; set; }

        public int GeneratePickupNumber()
        {
            // Generate a unique numeric pickup number (for example, using DateTime ticks)
            return DateTime.Now.Ticks.GetHashCode(); // Use the hash code of the ticks as an integer
        }
        public int PickupNumber { get; set; }
        public void GeneratePickupTime(DateTime orderTime)
        {
            PickupTime = orderTime.AddMinutes(30);  // Adjust as needed, for example, set pickup time 30 mins later
        }
    }

    public class ProductVM
    {
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
    }
}
