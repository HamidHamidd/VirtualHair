using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Index()
        {
            var list = await _context.Hairstyles
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        // GET: Hairstyles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hairstyle = await _context.Hairstyles.FirstOrDefaultAsync(m => m.Id == id);
            if (hairstyle == null) return NotFound();

            return View(hairstyle);
        }

        // GET: Hairstyles/Create
        public IActionResult Create() => View();

        // POST: Hairstyles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hairstyle hairstyle)
        {
            if (ModelState.IsValid)
            {
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
            return View(hairstyle);
        }

        // GET: Hairstyles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hairstyle = await _context.Hairstyles.FindAsync(id);
            if (hairstyle == null) return NotFound();

            return View(hairstyle);
        }

        // POST: Hairstyles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hairstyle hairstyle)
        {
            if (id != hairstyle.Id) return NotFound();

            if (ModelState.IsValid)
            {
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
                    hairstyle.ImagePath = existing.ImagePath; // запазваме старата снимка
                }

                _context.Update(hairstyle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hairstyle);
        }

        // GET: Hairstyles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hairstyle = await _context.Hairstyles.FirstOrDefaultAsync(m => m.Id == id);
            if (hairstyle == null) return NotFound();

            return View(hairstyle);
        }

        // POST: Hairstyles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hairstyle = await _context.Hairstyles.FindAsync(id);
            if (hairstyle != null)
            {
                // почистване на файла (по избор)
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
