using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VirtualHair.Models
{
    public class UserPhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get; set; }

        [Required]
        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        [Required]
        [MaxLength(50)]
        public string ContentType { get; set; } = "image/png";

        [MaxLength(255)]
        public string? FileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
