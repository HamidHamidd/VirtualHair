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
        public IActionResult Index(int? page)
        {
            const int pageSize = 6;
            int pageNumber = page ?? 1;

            var query = _context.FacialHairs
                .OrderByDescending(f => f.CreatedAt);

            var paged = query.ToList().ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        // GET: FacialHairs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var facialHair = await _context.FacialHairs.FirstOrDefaultAsync(f => f.Id == id);
            if (facialHair == null) return NotFound();

            return View(facialHair);
        }

        // GET: FacialHairs/Create (само Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST: FacialHairs/Create (само Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacialHair facialHair)
        {
            if (!ModelState.IsValid) return View(facialHair);

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

        // GET: FacialHairs/Edit/5 (само Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var facialHair = await _context.FacialHairs.FindAsync(id);
            if (facialHair == null) return NotFound();

            return View(facialHair);
        }

        // POST: FacialHairs/Edit/5 (само Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacialHair facialHair)
        {
            if (id != facialHair.Id) return NotFound();
            if (!ModelState.IsValid) return View(facialHair);

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
                facialHair.ImagePath = existing.ImagePath;
            }

            _context.Update(facialHair);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: FacialHairs/Delete/5 (само Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var facialHair = await _context.FacialHairs.FirstOrDefaultAsync(f => f.Id == id);
            if (facialHair == null) return NotFound();

            return View(facialHair);
        }

        // POST: FacialHairs/Delete/5 (само Admin)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
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
