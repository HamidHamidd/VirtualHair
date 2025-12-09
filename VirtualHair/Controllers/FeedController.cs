using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public FeedController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // FEED LIST
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // CREATE POST VIEW
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST SUBMIT
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile imageFile, string? description)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Please select an image.";
                return View();
            }

            string folder = Path.Combine(_env.WebRootPath, "uploads", "posts");
            Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var post = new Post
            {
                UserId = _userManager.GetUserId(User),
                ImagePath = "/uploads/posts/" + fileName,
                Description = description
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // LIKE / UNLIKE 
        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] dynamic data)
        {
            int postId = (int)data.postId;

            var userId = _userManager.GetUserId(User);
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null)
            {
                _context.Likes.Add(new Like
                {
                    UserId = userId,
                    PostId = postId
                });
            }
            else
            {
                _context.Likes.Remove(like);
            }

            await _context.SaveChangesAsync();

            var updatedLikes = await _context.Likes.CountAsync(l => l.PostId == postId);

            return Json(new { likes = updatedLikes });
        }

        // ADD COMMENT
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Empty comment");

            var comment = new Comment
            {
                PostId = postId,
                Text = text,
                UserId = _userManager.GetUserId(User)
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                text = comment.Text,
                user = _userManager.GetUserName(User),
                createdAt = comment.CreatedAt.ToString("g")
            });
        }

        // VIEW POST DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }
    }
}
