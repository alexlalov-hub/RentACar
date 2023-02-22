using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Models
{
    public class Image
    {
        public int Id { get; set; }

        [Required]
        public byte[] Bytes { get; set; }

        public string FileExtension { get; set; }
        public decimal Size { get; set; }

        [Required]
        public int? CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }
    }
}
