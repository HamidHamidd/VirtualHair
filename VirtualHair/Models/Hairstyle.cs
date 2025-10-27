using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class Hairstyle
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = "";

        public Gender Gender { get; set; } = Gender.Unisex;

        [MaxLength(40)]
        public string Length { get; set; } = "";

        public FadeType DefaultFade { get; set; } = FadeType.None;

        [MaxLength(7)]
        public string? DefaultColorHex { get; set; } 

        [Required, MaxLength(260)]
        public string ImageUrl { get; set; } = ""; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
