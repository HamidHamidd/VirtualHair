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
    public class FadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fades
        public async Task<IActionResult> Index()
        {
            return View(await _context.Fade.ToListAsync());
        }

        // GET: Fades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fade = await _context.Fade
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fade == null)
            {
                return NotFound();
            }

            return View(fade);
        }

        // GET: Fades/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ImageUrl,Type")] Fade fade)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fade);
        }

        // GET: Fades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fade = await _context.Fade.FindAsync(id);
            if (fade == null)
            {
                return NotFound();
            }
            return View(fade);
        }

        // POST: Fades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ImageUrl,Type")] Fade fade)
        {
            if (id != fade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FadeExists(fade.Id))
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
            return View(fade);
        }

        // GET: Fades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fade = await _context.Fade
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fade == null)
            {
                return NotFound();
            }

            return View(fade);
        }

        // POST: Fades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fade = await _context.Fade.FindAsync(id);
            if (fade != null)
            {
                _context.Fade.Remove(fade);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FadeExists(int id)
        {
            return _context.Fade.Any(e => e.Id == id);
        }
    }
}
