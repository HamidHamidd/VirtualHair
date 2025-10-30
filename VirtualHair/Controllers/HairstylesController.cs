using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtualHair.Data;
using VirtualHair.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace VirtualHair.Controllers
{
    [Authorize]
    public class HairstylesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadFolder = "wwwroot/uploads/hairstyles";

        public HairstylesController(ApplicationDbContext context)
        {
            _context = context;
            Directory.CreateDirectory(_uploadFolder);
        }

        // GET: Hairstyles
        public async Task<IActionResult> Index(int? page)
        {
            const int pageSize = 6;
            int pageNumber = page ?? 1;

            var query = _context.Hairstyles
                .OrderByDescending(h => h.CreatedAt);

            var paged = query.ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        // GET: Hairstyles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hairstyle = await _context.Hairstyles.FirstOrDefaultAsync(m => m.Id == id);
            if (hairstyle == null) return NotFound();

            return View(hairstyle);
        }

        // GET: Hairstyles/Create  (само Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST: Hairstyles/Create  (само Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hairstyle hairstyle)
        {
            if (!ModelState.IsValid) return View(hairstyle);

            if (hairstyle.ImageFile != null && hairstyle.ImageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(hairstyle.ImageFile.FileName)}";
                var savePath = Path.Combine(_uploadFolder, fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await hairstyle.ImageFile.CopyToAsync(stream);

                hairstyle.ImagePath = $"/uploads/hairstyles/{fileName}";
            }

            _context.Add(hairstyle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Hairstyles/Edit/5  (само Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hairstyle = await _context.Hairstyles.FindAsync(id);
            if (hairstyle == null) return NotFound();

            return View(hairstyle);
        }

        // POST: Hairstyles/Edit/5  (само Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hairstyle hairstyle)
        {
            if (id != hairstyle.Id) return NotFound();
            if (!ModelState.IsValid) return View(hairstyle);

            var existing = await _context.Hairstyles.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
            if (existing == null) return NotFound();

            if (hairstyle.ImageFile != null && hairstyle.ImageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(hairstyle.ImageFile.FileName)}";
                var savePath = Path.Combine(_uploadFolder, fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await hairstyle.ImageFile.CopyToAsync(stream);

                hairstyle.ImagePath = $"/uploads/hairstyles/{fileName}";
            }
            else
            {
                hairstyle.ImagePath = existing.ImagePath; 
            }

            _context.Update(hairstyle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Hairstyles/Delete/5  (само Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hairstyle = await _context.Hairstyles.FirstOrDefaultAsync(m => m.Id == id);
            if (hairstyle == null) return NotFound();

            return View(hairstyle);
        }

        // POST: Hairstyles/Delete/5  (само Admin)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hairstyle = await _context.Hairstyles.FindAsync(id);
            if (hairstyle != null)
            {
                if (!string.IsNullOrEmpty(hairstyle.ImagePath))
                {
                    var fullPath = Path.Combine("wwwroot", hairstyle.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.Hairstyles.Remove(hairstyle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HairstyleExists(int id) => _context.Hairstyles.Any(e => e.Id == id);
    }
}
