using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // ================= INDEX =================

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var looks = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(looks);
        }

        // ================= DETAILS =================

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
                return NotFound();

            return View(look);
        }

        // ================= CREATE =================

        public async Task<IActionResult> Create()
        {
            ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
            ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserHairstyle model)
        {
            var userId = _userManager.GetUserId(User);

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
                ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
                ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
                return View(model);
            }

            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;

            _context.UserHairstyles.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT (SAFE UPDATE) =================

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

            var userId = _userManager.GetUserId(User);

            var existing = await _context.UserHairstyles
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
                ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
                return View(model);
            }

            // 🔥 ONLY update editable fields
            existing.Title = model.Title;
            existing.HairstyleId = model.HairstyleId;
            existing.FacialHairId = model.FacialHairId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Changes saved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================

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

            return RedirectToAction(nameof(Index));
        }

        // ================= PREVIEW =================

        public async Task<IActionResult> Preview()
        {
            ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
            ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
            return View();
        }

        // ================= SAVE LOOK =================

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

            var title = (req.Title ?? "").Trim();

            var duplicate = await _context.UserHairstyles
                .AnyAsync(x => x.UserId == userId && x.Title.ToLower() == title.ToLower());

            if (duplicate)
                return BadRequest(new { success = false, message = "Duplicate title." });

            var base64Data = Regex.Replace(req.ImageData, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            var bytes = Convert.FromBase64String(base64Data);

            var folder = Path.Combine(_environment.WebRootPath, "uploads", "userlooks");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(folder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            var imagePathForDb = $"/uploads/userlooks/{fileName}";

            var look = new UserHairstyle
            {
                UserId = userId,
                Title = title,
                ImagePath = imagePathForDb,
                HairstyleId = req.HairstyleId ?? 0,
                FacialHairId = req.FacialHairId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserHairstyles.Add(look);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
