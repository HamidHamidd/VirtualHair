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

            // Get usernames mapping
            var allUserIds = posts.Select(p => p.UserId)
                .Concat(posts.SelectMany(p => p.Comments).Select(c => c.UserId))
                .Distinct();
            
            ViewBag.UserNames = await _context.Users
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

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
        public async Task<IActionResult> Create(IFormFile imageFile, string? description, string? tags)
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
                Description = description,
                Tags = tags
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // LIKE / DISLIKE 
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
                _context.Likes.Add(new Like
                {
                    UserId = userId,
                    PostId = postId,
                    IsDislike = isDislike
                });
            }
            else
            {
                if (existingLike.IsDislike == isDislike)
                {
                    _context.Likes.Remove(existingLike);
                }
                else
                {
                    existingLike.IsDislike = isDislike;
                }
            }

            await _context.SaveChangesAsync();

            var updatedLikes = await _context.Likes.CountAsync(l => l.PostId == postId && !l.IsDislike);
            var updatedDislikes = await _context.Likes.CountAsync(l => l.PostId == postId && l.IsDislike);

            return Json(new { likes = updatedLikes, dislikes = updatedDislikes });
        }

        // RATE POST
        [HttpPost]
        public async Task<IActionResult> RatePost([FromBody] RatePostDto data)
        {
            if (data == null) return BadRequest();

            int postId = data.PostId;
            int score = data.Score;

            if (score < 1) score = 1;
            if (score > 5) score = 5;

            var userId = _userManager.GetUserId(User);
            var existingRating = await _context.Ratings.FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

            if (existingRating == null)
            {
                _context.Ratings.Add(new Rating
                {
                    UserId = userId,
                    PostId = postId,
                    Score = score
                });
            }
            else
            {
                existingRating.Score = score;
            }

            await _context.SaveChangesAsync();

            var avgRating = await _context.Ratings.Where(r => r.PostId == postId).AverageAsync(r => (double?)r.Score) ?? 0;
            var totalRatings = await _context.Ratings.CountAsync(r => r.PostId == postId);

            return Json(new { averageRating = Math.Round(avgRating, 1), totalRatings = totalRatings });
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
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // MY POSTS
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Include(p => p.Ratings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.UserNames = new Dictionary<string, string> { { userId, _userManager.GetUserName(User) } };
            ViewBag.IsMyPosts = true;
            return View("Index", posts);
        }

        // EDIT POST (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != _userManager.GetUserId(User))
                return Forbid();

            return View(post);
        }

        // EDIT POST (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string description, string tags)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != _userManager.GetUserId(User))
                return Forbid();

            post.Description = description;
            post.Tags = tags;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // DELETE POST
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.UserId != _userManager.GetUserId(User))
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }

    public class ToggleLikeDto
    {
        public int PostId { get; set; }
        public bool IsDislike { get; set; }
    }

    public class RatePostDto
    {
        public int PostId { get; set; }
        public int Score { get; set; }
    }
}
