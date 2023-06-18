using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAppMain.Data;
using WebAppMain.Models;

namespace WebAppMain.Controllers
{
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ClassController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Class
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

            var usersWithRole = await userManager.GetUsersInRoleAsync("Ученик");
            var studentCount = usersWithRole.Count();


            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewData["CurrentFilter"] = searchString;


            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            IQueryable<Class> predmeti = _context.Class.Include(c => c.ApplicationUser);

            if (predmeti.Any())
            {
                foreach (var classItem in predmeti)
                {
                    var teacher = classItem.ApplicationUser.FirstOrDefault(u => userManager.IsInRoleAsync(u, "Учитель").Result);
                    var students = classItem.ApplicationUser.Count(u => userManager.IsInRoleAsync(u, "Ученик").Result);

                    ViewData[$"TeacherName_{classItem.Id_Class}"] = teacher != null ? $"{teacher.Surname} {teacher.Name} {teacher.Patronymic}" : "Нет учителя";
                    ViewData[$"StudentCount_{classItem.Id_Class}"] = students;
                }
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                predmeti = predmeti.Where(p => p.Title_Class.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_asc":
                    predmeti = predmeti.OrderBy(p => p.Title_Class);
                    break;
                case "title_desc":
                    predmeti = predmeti.OrderByDescending(p => p.Title_Class);
                    break;
                default:
                    predmeti = predmeti.OrderBy(p => p.Title_Class);
                    break;

            }

            int pageSize = 7;
            return View(await PaginatedList<Class>.CreateAsync(predmeti.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Class/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Class
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id_Class == id);

            if (@class == null)
            {
                return NotFound();
            }

            // Filter students by class id
            var students = await _userManager.GetUsersInRoleAsync("Ученик");
            students = students.Where(s => s.Id_Class == id).ToList();

            return View(students);
        }



        // GET: Class/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Class/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id_Class,Title_Class")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@class);
        }

        // GET: Class/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Class.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            return View(@class);
        }

        // POST: Class/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Class,Title_Class")] Class @class)
        {
            if (id != @class.Id_Class)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id_Class))
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
            return View(@class);
        }

        // GET: Class/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Class
                .FirstOrDefaultAsync(m => m.Id_Class == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Class/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Class Class = await _context.Class
                .Include(h => h.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id_Class == id);

            if (Class != null)
            {
                if (Class.ApplicationUser.Count == 0)
                {
                    var @class = await _context.Class.FindAsync(id);
                    _context.Class.Remove(@class);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClassExists(int id)
        {
            return _context.Class.Any(e => e.Id_Class == id);
        }
    }
}
