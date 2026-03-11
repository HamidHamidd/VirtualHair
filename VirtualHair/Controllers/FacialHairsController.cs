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
        public FacialHairsController(ApplicationDbContext context)
        {
            _context = context;
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
                using var ms = new MemoryStream();
                await facialHair.ImageFile.CopyToAsync(ms);

                facialHair.ImageData = ms.ToArray();
                facialHair.ContentType = facialHair.ImageFile.ContentType;
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
                using var ms = new MemoryStream();
                await facialHair.ImageFile.CopyToAsync(ms);

                facialHair.ImageData = ms.ToArray();
                facialHair.ContentType = facialHair.ImageFile.ContentType;
            }
            else
            {
                facialHair.ImageData = existing.ImageData;
                facialHair.ContentType = existing.ContentType;
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
                _context.FacialHairs.Remove(facialHair);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Image(int id)
        {
            var item = await _context.FacialHairs.FindAsync(id);
            if (item == null || item.ImageData == null)
                return NotFound();

            return File(item.ImageData, item.ContentType ?? "image/png");
        }

        private bool FacialHairExists(int id) => _context.FacialHairs.Any(e => e.Id == id);
    }
}
