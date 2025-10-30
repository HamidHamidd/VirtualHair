using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VirtualHair.Models
{
    public class UserHairstyle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get; set; }

        [Required]
        public int HairstyleId { get; set; }

        [ForeignKey(nameof(HairstyleId))]
        public Hairstyle? Hairstyle { get; set; }

        public int? FacialHairId { get; set; }

        [ForeignKey(nameof(FacialHairId))]
        public FacialHair? FacialHair { get; set; }

        public int? UserPhotoId { get; set; }

        [ForeignKey(nameof(UserPhotoId))]
        public UserPhoto? UserPhoto { get; set; }   

        [MaxLength(100)]
        public string? Title { get; set; }

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
