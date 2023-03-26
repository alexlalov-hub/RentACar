using System.ComponentModel.DataAnnotations;

namespace RentACar.Models
{
    public class CarEditViewModel
    {
        public int Id { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        [Range(1900, 2023)]
        public int ManufactureYear { get; set; }

        public decimal DailyPrice { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }
    }
}
