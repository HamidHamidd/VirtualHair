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
    public class FacialHairsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadFolder = "wwwroot/uploads/facialhairs";

        public FacialHairsController(ApplicationDbContext context)
        {
            _context = context;
            Directory.CreateDirectory(_uploadFolder);
        }

        // GET: FacialHairs
        public async Task<IActionResult> Index()
        {
            var list = await _context.FacialHairs
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        // GET: FacialHairs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var facialHair = await _context.FacialHairs.FirstOrDefaultAsync(m => m.Id == id);
            if (facialHair == null) return NotFound();

            return View(facialHair);
        }

        // GET: FacialHairs/Create
        public IActionResult Create() => View();

        // POST: FacialHairs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacialHair facialHair)
        {
            if (ModelState.IsValid)
            {
                if (facialHair.ImageFile != null && facialHair.ImageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(facialHair.ImageFile.FileName)}";
                    var savePath = Path.Combine(_uploadFolder, fileName);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await facialHair.ImageFile.CopyToAsync(stream);

                    facialHair.ImagePath = $"/uploads/facialhairs/{fileName}";
                }

                facialHair.CreatedAt = DateTime.UtcNow;
                _context.Add(facialHair);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(facialHair);
        }

        // GET: FacialHairs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var facialHair = await _context.FacialHairs.FindAsync(id);
            if (facialHair == null) return NotFound();

            return View(facialHair);
        }

        // POST: FacialHairs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacialHair facialHair)
        {
            if (id != facialHair.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.FacialHairs.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
                if (existing == null) return NotFound();

                if (facialHair.ImageFile != null && facialHair.ImageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(facialHair.ImageFile.FileName)}";
                    var savePath = Path.Combine(_uploadFolder, fileName);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await facialHair.ImageFile.CopyToAsync(stream);

                    facialHair.ImagePath = $"/uploads/facialhairs/{fileName}";
                }
                else
                {
                    facialHair.ImagePath = existing.ImagePath; // запазваме старата снимка
                }

                _context.Update(facialHair);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(facialHair);
        }

        // GET: FacialHairs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var facialHair = await _context.FacialHairs.FirstOrDefaultAsync(m => m.Id == id);
            if (facialHair == null) return NotFound();

            return View(facialHair);
        }

        // POST: FacialHairs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facialHair = await _context.FacialHairs.FindAsync(id);
            if (facialHair != null)
            {
                if (!string.IsNullOrEmpty(facialHair.ImagePath))
                {
                    var fullPath = Path.Combine("wwwroot", facialHair.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.FacialHairs.Remove(facialHair);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FacialHairExists(int id) => _context.FacialHairs.Any(e => e.Id == id);
    }
}
