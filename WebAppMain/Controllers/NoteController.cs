using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using WebAppMain.Data;
using WebAppMain.Models;
using System.Linq;

namespace WebAppMain.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public NoteController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var notes = await _dbContext.Notes
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(notes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                note.CreatedAt = DateTime.Now;
                note.UserId = user.Id;
                _dbContext.Notes.Add(note);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return PartialView("Create", note);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var note = await _dbContext.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    note.CreatedAt = DateTime.Now;
                    note.UserId = user.Id;
                    _dbContext.Update(note);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(note);
        }

        [HttpGet]
        public IActionResult DeleteConfirmed()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _dbContext.Notes.FindAsync(id);
            if (ModelState.IsValid)
            {
                
                if (note == null)
                {
                    return NotFound();
                }
                _dbContext.Notes.Remove(note);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return PartialView("Delete", note);
        }

        private bool NoteExists(int id)
        {
            return _dbContext.Notes.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predmet = await _dbContext.Notes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (predmet == null)
            {
                return NotFound();
            }

            return View(predmet);
        }
    }

}
