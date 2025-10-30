using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class UserHairstylesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserHairstylesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserHairstyles
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var looks = await _context.UserPhotos
                .Join(_context.UserPhotos,
                    u => u.UserId,
                    p => p.UserId,
                    (u, p) => u)
                .ToListAsync();

            var userLooks = await _context.UserPhotos
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var hairstyles = await _context.UserPhotos
                .ToListAsync();

            var items = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(items);
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

            if (ModelState.IsValid)
            {
                model.UserId = userId;
                model.CreatedAt = DateTime.UtcNow;

                _context.UserHairstyles.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Ако моделът не е валиден — презареждаме dropdown-ите
            ViewBag.Hairstyles = await _context.Hairstyles.ToListAsync();
            ViewBag.FacialHairs = await _context.FacialHairs.ToListAsync();
            return View(model);
        }

        // GET: UserHairstyles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .Include(x => x.UserPhoto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // GET: UserHairstyles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.UserHairstyles
                .Include(x => x.Hairstyle)
                .Include(x => x.FacialHair)
                .Include(x => x.UserPhoto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();

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
            return RedirectToAction(nameof(Index));
        }
    }
}
