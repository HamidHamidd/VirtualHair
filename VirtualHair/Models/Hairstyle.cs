using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class Hairstyle
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public string Gender { get; set; }  
        public string Length { get; set; }   
        public string Color { get; set; }    
    }
}
