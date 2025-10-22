using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class Fade
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public byte[] ImageData { get; set; }  

        public string Type { get; set; }
    }
}
