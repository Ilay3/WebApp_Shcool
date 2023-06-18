using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppMain.Data;
using WebAppMain.Models;

namespace WebAppMain.Controllers
{
    public class PredmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Predmet
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["CurrentFilter"] = searchString;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            IQueryable<Predmet> predmeti = _context.Predmet;

            if (!String.IsNullOrEmpty(searchString))
            {
                predmeti = predmeti.Where(p => p.Title_Predmet.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_asc":
                    predmeti = predmeti.OrderBy(p => p.Title_Predmet);
                    break;
                case "title_desc":
                    predmeti = predmeti.OrderByDescending(p => p.Title_Predmet);
                    break;
    
                case "date_asc":
                    predmeti = predmeti.OrderBy(p => p.Study_period);
                    break;
                case "date_desc":
                    predmeti = predmeti.OrderByDescending(p => p.Study_period);
                    break;
                default:
                    predmeti = predmeti.OrderBy(p => p.Title_Predmet);
                    break;

            }

            int pageSize = 7;
            return View(await PaginatedList<Predmet>.CreateAsync(predmeti.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // GET: Predmet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predmet = await _context.Predmet
                .FirstOrDefaultAsync(m => m.Id_Predmet == id);
            if (predmet == null)
            {
                return NotFound();
            }

            return View(predmet);
        }

        // GET: Predmet/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Predmet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Predmet,Title_Predmet,Description_Predmet,Study_period")] Predmet predmet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(predmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(predmet);
        }

        // GET: Predmet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predmet = await _context.Predmet.FindAsync(id);
            if (predmet == null)
            {
                return NotFound();
            }
            return View(predmet);
        }

        // POST: Predmet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Predmet,Title_Predmet,Description_Predmet,Study_period")] Predmet predmet)
        {
            if (id != predmet.Id_Predmet)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(predmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PredmetExists(predmet.Id_Predmet))
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
            return View(predmet);
        }

        // GET: Predmet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predmet = await _context.Predmet
                .FirstOrDefaultAsync(m => m.Id_Predmet == id);
            if (predmet == null)
            {
                return NotFound();
            }

            return View(predmet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id != null)
            {
                Predmet Predmet = await _context.Predmet
                    .Include(h => h.JournalPredmets)
                    .FirstOrDefaultAsync(p => p.Id_Predmet == id);

                if (Predmet != null)
                {
                    if (Predmet.JournalPredmets.Count == 0)
                    {
                        var predmet = await _context.Predmet.FindAsync(id);
                        _context.Predmet.Remove(predmet);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }


        private bool PredmetExists(int id)
        {
            return _context.Predmet.Any(e => e.Id_Predmet == id);
        }
    }
}
