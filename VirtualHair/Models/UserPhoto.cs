using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class UserPhoto
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required, MaxLength(260)]
        public string StoredPath { get; set; } = ""; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
