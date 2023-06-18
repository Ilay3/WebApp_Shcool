using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WebAppMain.Data;
using WebAppMain.Enums;
using WebAppMain.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Http;
using MimeKit;

namespace WebAppMain.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var assignment = await _context.Assignment.FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound("Assignment not found.");
            }

            if (assignment.File == null || assignment.File.Length == 0)
            {
                return NotFound("File not found.");
            }

            /// Save file to temporary folder with .docx extension
            var filePath = Path.ChangeExtension(Path.GetTempFileName(), ".docx");
            await System.IO.File.WriteAllBytesAsync(filePath, assignment.File);

            // Create file info object from temporary file path
            var fileInfo = new FileInfo(filePath);

            // Return file for download
            return PhysicalFile(fileInfo.FullName, "application/octet-stream", fileInfo.Name);

        }


        // GET: Assignments/Create
        public IActionResult Create()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var @class = _context.Class.FirstOrDefault(c => c.Id_Class == user.Id_Class);
            if (@class == null)
            {
                return NotFound();
            }


            var subjects = _context.Predmet.ToList(); // Retrieve the list of subjects

            ViewBag.Subjects = subjects; // Pass the subjects list to the view

            return View();
        }

        // POST: Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,File,IsPublic")] Assignment assignment, IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            var @class = await _context.Class.FirstOrDefaultAsync(c => c.Id_Class == user.Id_Class);
            if (@class == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0) // Проверьте, что файл загружен
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        assignment.File = memoryStream.ToArray(); // Сохраните файл в свойстве модели File
                    }
                }

                assignment.Date = DateTime.Now;
                assignment.Class = @class;
                _context.Add(assignment);
                await _context.SaveChangesAsync();

                var notificationController = new NotificationController(_context, _userManager);
                notificationController.CreateAssignmentNotification(@class.Id_Class);

                return RedirectToAction(nameof(Index));
            }
            return View(assignment);
        }

        [HttpGet]
        [Authorize(Roles = "Учитель")]
        public async Task<IActionResult> EditIsPublic(int id, bool isPublic)
        {
            var assignment = await _context.Assignment.FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound("Assignment not found.");
            }

            assignment.IsPublic = isPublic;
            _context.Update(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var @class = await _context.Class.FirstOrDefaultAsync(c => c.Id_Class == user.Id_Class);
            if (@class == null)
            {
                return NotFound();
            }

            var assignments = await _context.Assignment
                .Where(a => a.ClassId == @class.Id_Class && (a.IsPublic || User.IsInRole("Учитель")))
                .ToListAsync();

            return View(assignments);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Учитель")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.Assignment.FirstOrDefaultAsync(a => a.Id == id);

            if (ModelState.IsValid)
            {
                if (assignment == null)
                {
                    return NotFound("Assignment not found.");
                }
                _context.Assignment.Remove(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
             }

            return PartialView("Delete", assignment);
        }

       
    }

}
