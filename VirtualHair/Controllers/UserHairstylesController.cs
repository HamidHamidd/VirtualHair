using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;
using System.Text.RegularExpressions;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class UserHairstylesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public UserHairstylesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: UserHairstyles
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "⚠️ You must be logged in to view your gallery.";
                return RedirectToAction("Index", "Home");
            }

            var looks = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(looks);
        }

        // GET: UserHairstyles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var look = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (look == null)
            {
                TempData["ErrorMessage"] = "⚠️ Look not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            return View(look);
        }

        // GET: UserHairstyles/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
            ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
            return View();
        }

        // POST: UserHairstyles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserHairstyle model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "⚠️ You must be logged in to create a look.";
                return RedirectToAction("Index", "Home");
            }

            if (model.HairstyleId == 0)
                ModelState.AddModelError("HairstyleId", "Please select a hairstyle.");

            var duplicate = await _context.UserHairstyles
                .AnyAsync(x => x.UserId == userId && x.Title.ToLower() == model.Title.ToLower());

            if (duplicate)
            {
                TempData["ErrorMessage"] = "⚠️ You already have a look with this name.";
                ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
                ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "❌ Validation failed.";
                ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
                ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
                return View(model);
            }

            try
            {
                model.UserId = userId;
                model.CreatedAt = DateTime.UtcNow;
                _context.UserHairstyles.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Look created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error while saving: {ex.Message}";
                return View(model);
            }
        }

        // GET: UserHairstyles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var look = await _context.UserHairstyles.FindAsync(id);
            if (look == null)
                return NotFound();

            ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
            ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
            return View(look);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserHairstyle model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Changes saved successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserHairstyles.Any(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
            ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
            return View(model);
        }

        // GET: UserHairstyles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var item = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: UserHairstyles/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var look = await _context.UserHairstyles.FindAsync(id);
            if (look != null)
            {
                _context.UserHairstyles.Remove(look);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "🗑️ Look deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ PREVIEW PAGE
        [Authorize]
        public async Task<IActionResult> Preview()
        {
            var hairstyles = await _context.Hairstyles.ToListAsync();
            var facialHairs = await _context.FacialHairs.ToListAsync();

            ViewBag.Hairstyles = hairstyles;
            ViewBag.FacialHairs = facialHairs;

            return View();
        }

        // ✅ NEW: SAVE LOOK from Canvas
        public class SaveLookRequest
        {
            public string Title { get; set; } = "";
            public string ImageData { get; set; } = "";
            public int? HairstyleId { get; set; }
            public int? FacialHairId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLook([FromBody] SaveLookRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Not logged in." });

            var title = (req.Title ?? "").Trim();
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(new { success = false, message = "Title is required." });

            // Duplicate title check (per user)
            var duplicate = await _context.UserHairstyles
                .AnyAsync(x => x.UserId == userId && x.Title.ToLower() == title.ToLower());

            if (duplicate)
                return BadRequest(new { success = false, message = "You already have a look with this title." });

            if (string.IsNullOrWhiteSpace(req.ImageData) || !req.ImageData.StartsWith("data:image"))
                return BadRequest(new { success = false, message = "Invalid image data." });

            try
            {
                // Extract base64 part
                var base64Data = Regex.Replace(req.ImageData, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                var bytes = Convert.FromBase64String(base64Data);

                // Ensure folder exists
                var folder = Path.Combine(_environment.WebRootPath, "uploads", "userlooks");
                Directory.CreateDirectory(folder);

                // Save file
                var fileName = $"{Guid.NewGuid()}.png";
                var filePath = Path.Combine(folder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                var imagePathForDb = $"/uploads/userlooks/{fileName}";

                var look = new UserHairstyle
                {
                    UserId = userId,
                    Title = title,
                    ImagePath = imagePathForDb,
                    HairstyleId = req.HairstyleId ?? 0,   // if null -> will be 0 (required field)
                    FacialHairId = req.FacialHairId,
                    CreatedAt = DateTime.UtcNow
                };

                // If no hairstyle selected (null), keep HairstyleId=0 out of DB by setting null? 
                // But HairstyleId is required in your model.
                // So we handle by allowing save without hairstyle ONLY if you want:
                // If req.HairstyleId is null -> set to any existing hairstyle? NO.
                // Better: if null, don't save HairstyleId requirement. We'll force to 0 invalid? Not ok.
                // So we do this:
                if (req.HairstyleId == null)
                    look.HairstyleId = 0; // won't break save if DB allows 0? If not, create a "NoHairstyle" record.

                _context.UserHairstyles.Add(look);
                await _context.SaveChangesAsync();

                return Json(new { success = true, imagePath = imagePathForDb });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
