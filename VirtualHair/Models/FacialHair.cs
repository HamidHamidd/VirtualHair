using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace VirtualHair.Models
{
    public class FacialHair
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = "";

        [MaxLength(260)]
        public string? ImagePath { get; set; }  

        [NotMapped]
        public IFormFile? ImageFile { get; set; }  

        [MaxLength(7)]
        public string? DefaultColorHex { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
