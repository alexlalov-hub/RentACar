using System.ComponentModel.DataAnnotations;

namespace RentACar.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
