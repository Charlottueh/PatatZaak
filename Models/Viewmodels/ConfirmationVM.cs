using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class ConfirmationVM
    {
        public int OrderNumber { get; set; }
        public int PickupNumber { get; set; }  // Maak dit een int i.p.v. string
        public string PickupTime { get; set; } // PickupTime als string blijft oké
        public decimal TotalPrice { get; set; }
        public IEnumerable<ProductVM> Products { get; set; }

        public void GeneratePickupTime(DateTime orderTime)
        {
            PickupTime = orderTime.AddMinutes(20).ToString("HH:mm"); // Correcte conversie
        }

        public void GeneratePickupNumber()
        {
            var random = new Random();
            PickupNumber = random.Next(1000, 9999); // Gebruik int i.p.v. string
        }
    }
    public class ProductVM
    {
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
    }
}
