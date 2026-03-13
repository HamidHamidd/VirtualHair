using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace VirtualHair.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalHairstyles { get; set; }
        public int TotalFacialHairs { get; set; }
        public int TotalPosts { get; set; }
        public int TotalMessages { get; set; }
        public int TotalPhotos { get; set; }
        public int TotalSavedLooks { get; set; }
        
        public List<IdentityUser> RecentUsers { get; set; } = new();
        public List<Post> RecentPosts { get; set; } = new();
    }
}
