using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualHair.Data;
using VirtualHair.Models;

namespace VirtualHair.Controllers
{
    public class HairstylesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HairstylesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hairstyles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Hairstyle.ToListAsync());
        }

        // GET: Hairstyles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hairstyle = await _context.Hairstyle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hairstyle == null)
            {
                return NotFound();
            }

            return View(hairstyle);
        }

        // GET: Hairstyles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Hairstyles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ImageUrl,Gender,Length,Color")] Hairstyle hairstyle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hairstyle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hairstyle);
        }

        // GET: Hairstyles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hairstyle = await _context.Hairstyle.FindAsync(id);
            if (hairstyle == null)
            {
                return NotFound();
            }
            return View(hairstyle);
        }

        // POST: Hairstyles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ImageUrl,Gender,Length,Color")] Hairstyle hairstyle)
        {
            if (id != hairstyle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hairstyle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HairstyleExists(hairstyle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hairstyle);
        }

        // GET: Hairstyles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hairstyle = await _context.Hairstyle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hairstyle == null)
            {
                return NotFound();
            }

            return View(hairstyle);
        }

        // POST: Hairstyles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hairstyle = await _context.Hairstyle.FindAsync(id);
            if (hairstyle != null)
            {
                _context.Hairstyle.Remove(hairstyle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HairstyleExists(int id)
        {
            return _context.Hairstyle.Any(e => e.Id == id);
        }
    }
}
