using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace VirtualHair.Models
{
    [Index(nameof(UserId), nameof(Title), IsUnique = true)] // ✅ Не позволява дублиране на име при същия User
    public class UserHairstyle
    {
        [Key]
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        public string? UserId { get; set; }

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
        [Required(ErrorMessage = "Please enter a name for your look.")]
        public string Title { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
