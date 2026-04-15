using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public TimelineController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // TIMELINE LIST
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Include(p => p.Ratings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ViewBag.ChatUsers = await _userManager.Users.Where(u => u.Id != currentUser.Id).ToListAsync();
            }

            var allUserIds = posts.Select(p => p.UserId)
                .Concat(posts.SelectMany(p => p.Comments).Select(c => c.UserId))
                .Distinct();
            
            ViewBag.UserNames = await _context.Users
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            return View(posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile imageFile, string? description, string? tags, string? croppedImageData)
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

            if (!string.IsNullOrEmpty(croppedImageData))
            {
                var base64Data = croppedImageData.Contains(",") ? croppedImageData.Split(',')[1] : croppedImageData;
                var bytes = Convert.FromBase64String(base64Data);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
            }
            else
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
            }

            var post = new Post
            {
                UserId = _userManager.GetUserId(User),
                ImagePath = "/uploads/posts/" + fileName,
                Description = description,
                Tags = tags
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeDto data)
        {
            if (data == null) return BadRequest();
            int postId = data.PostId;
            bool isDislike = data.IsDislike;
            var userId = _userManager.GetUserId(User);
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                _context.Likes.Add(new Like { UserId = userId, PostId = postId, IsDislike = isDislike });
            }
            else
            {
                if (existingLike.IsDislike == isDislike) _context.Likes.Remove(existingLike);
                else existingLike.IsDislike = isDislike;
            }
            await _context.SaveChangesAsync();
            return Json(new { likes = await _context.Likes.CountAsync(l => l.PostId == postId && !l.IsDislike), dislikes = await _context.Likes.CountAsync(l => l.PostId == postId && l.IsDislike) });
        }

        [HttpPost]
        public async Task<IActionResult> RatePost([FromBody] RatePostDto data)
        {
            if (data == null) return BadRequest();
            int postId = data.PostId;
            int score = Math.Clamp(data.Score, 1, 5);
            var userId = _userManager.GetUserId(User);
            var existingRating = await _context.Ratings.FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

            if (existingRating == null) _context.Ratings.Add(new Rating { UserId = userId, PostId = postId, Score = score });
            else existingRating.Score = score;

            await _context.SaveChangesAsync();
            return Json(new { averageRating = Math.Round(await _context.Ratings.Where(r => r.PostId == postId).AverageAsync(r => (double?)r.Score) ?? 0, 1), totalRatings = await _context.Ratings.CountAsync(r => r.PostId == postId) });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return BadRequest("Empty comment");
            var comment = new Comment { PostId = postId, Text = text, UserId = _userManager.GetUserId(User) };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = comment.Id, text = comment.Text, user = _userManager.GetUserName(User), createdAt = comment.CreatedAt.ToString("g") });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            if (comment.UserId != _userManager.GetUserId(User)) return Forbid();
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include(p => p.Ratings).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);
            var posts = await _context.Posts.Where(p => p.UserId == userId).Include(p => p.Comments).Include(p => p.Likes).Include(p => p.Ratings).OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewBag.UserNames = new Dictionary<string, string> { { userId, _userManager.GetUserName(User) } };
            ViewBag.IsMyPosts = true;
            return View("Index", posts);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != _userManager.GetUserId(User)) return Forbid();
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string description, string tags)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != _userManager.GetUserId(User)) return Forbid();
            post.Description = description; post.Tags = tags;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != _userManager.GetUserId(User)) return Forbid();
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostHairstyle(int hairstyleId)
        {
            var userId = _userManager.GetUserId(User);
            var hairstyle = await _context.UserHairstyles.FirstOrDefaultAsync(h => h.Id == hairstyleId && h.UserId == userId);
            if (hairstyle == null || hairstyle.ImageData == null)
            {
                TempData["Error"] = "Hairstyle not found or image data missing.";
                return RedirectToAction("Index", "UserHairstyles");
            }
            string folder = Path.Combine(_env.WebRootPath, "uploads", "posts");
            Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString() + ".png";
            string filePath = Path.Combine(folder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, hairstyle.ImageData);
            var post = new Post { UserId = userId, ImagePath = "/uploads/posts/" + fileName, Description = $"Created using {hairstyle.Title ?? "VirtualHair"}", Tags = "VirtualHair, AiStyle" };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Successfully shared to Hub!";
            return RedirectToAction("Index");
        }
    }

    public class ToggleLikeDto { public int PostId { get; set; } public bool IsDislike { get; set; } }
    public class RatePostDto { public int PostId { get; set; } public int Score { get; set; } }
}
