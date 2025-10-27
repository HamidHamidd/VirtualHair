using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class FacialHair
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = "";

        [Required, MaxLength(260)]
        public string ImageUrl { get; set; } = ""; 

        [MaxLength(7)]
        public string? DefaultColorHex { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
