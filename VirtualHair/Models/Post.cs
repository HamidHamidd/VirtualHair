using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VirtualHair.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
