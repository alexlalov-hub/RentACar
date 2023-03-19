using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Models
{
    public class RentedCar
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int LocationTakenId { get; set; }

        [Required]
        public int LocationReturnedId { get; set; }

        [Required]
        public DateTime RentedDate { get; set; }

        [Required]
        public DateTime ReturnedDate { get ; set; }

        public decimal FinalPrice { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("LocationTakenId")]
        public Location? LocationTaken { get; set; }

        [ForeignKey("LocationReturnedId")]
        public Location? LocationReturned { get; set; }
    }
}
