using System;
using Microsoft.AspNetCore.Identity;

namespace VirtualHair.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string UserId { get; set; }

        public int Score { get; set; } // 1-5 or 1-10

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
