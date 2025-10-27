using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class SavedLook
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public int UserPhotoId { get; set; }
        public UserPhoto? UserPhoto { get; set; }

        public int? HairstyleId { get; set; }
        public Hairstyle? Hairstyle { get; set; }

        public int? FacialHairId { get; set; }
        public FacialHair? FacialHair { get; set; }

        [MaxLength(7)]
        public string? ColorHex { get; set; }

        public FadeType? Fade { get; set; }

        [MaxLength(120)]
        public string? Name { get; set; }

        public string? AdjustmentsJson { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
