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

        public void GeneratePickupTime(DateTime orderTime)
        {
            PickupTime = orderTime.AddMinutes(20).ToString("HH:mm"); // Bijv. "15:30"
        }

        public void GeneratePickupNumber()
        {
            var random = new Random();
            PickupNumber = random.Next(1000, 9999).ToString(); // Willekeurig 4-cijferig nummer
        }
    }
}
