using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required]
        public string Brand { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        [Range(1900, 2023)]
        public int ManufactureYear { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [Required]
        public float DailyPrice { get; set; }

        public string Description { get; set; }

        [Required]
        public bool IsRented { get; set; } = false;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Image>? Images { get; set; }

        [FromForm]
        [NotMapped]
        public IFormFileCollection Files { get; set; }
    }
}
