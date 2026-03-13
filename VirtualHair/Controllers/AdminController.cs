using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualHair.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalHairstyles = await _context.Hairstyles.CountAsync(),
                TotalFacialHairs = await _context.FacialHairs.CountAsync(),
                TotalPosts = await _context.Posts.CountAsync(),
                TotalMessages = await _context.Messages.CountAsync(),
                TotalPhotos = await _context.UserPhotos.CountAsync(),
                TotalSavedLooks = await _context.SavedLooks.CountAsync(),
                RecentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync(),
                RecentPosts = await _context.Posts.OrderByDescending(p => p.CreatedAt).Take(5).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Posts()
        {
            var posts = await _context.Posts
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Post deleted successfully.";
            }
            return RedirectToAction(nameof(Posts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Prevent deleting self
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser.Id == user.Id)
                {
                    TempData["Error"] = "You cannot delete yourself.";
                    return RedirectToAction(nameof(Users));
                }

                await _userManager.DeleteAsync(user);
                TempData["Success"] = "User deleted successfully.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
