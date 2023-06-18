using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WebAppMain.Data;
using WebAppMain.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAppMain.Controllers
{
    public class StudentController : Controller
    {
        private ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public StudentController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = applicationDbContext;
        }


        public async Task<IActionResult> IndexAsync(string searchString, int? page, string currentFilter, string className)
        {
            var classes = await db.Class.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();
            var users = await _userManager.Users.ToListAsync();

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => !string.IsNullOrEmpty(u.Name) && u.Name.Contains(searchString)
                                        || !string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(searchString)).ToList();
            }

            if (!string.IsNullOrEmpty(className))
            {
                if (className == "NoClass")
                {
                    users = users.Where(u => u.Class == null).ToList();
                }
                else
                {
                    users = users.Where(u => u.Class != null && u.Class.Title_Class != null && u.Class.Title_Class.Contains(className)).ToList();
                }
            }

            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Ученик");
            if (role != null)
            {
                users = users.Where(u => _userManager.IsInRoleAsync(u, role.Name).Result).ToList();
            }

            int pageSize = 7;
            int pageNumber = page ?? 1;
            int totalUsers = users.Count();
            int totalPages = (int)Math.Ceiling((decimal)totalUsers / pageSize);

            var model = new UserListViewModel
            {
                Users = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                SearchString = searchString,
                ClassName = className,
                RoleManager = _roleManager,
                UserManager = _userManager,
                Classes = classes,
                Roles = roles
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var classList = await db.Class.Select(x => new SelectListItem
            {
                Value = x.Id_Class.ToString(),
                Text = x.Title_Class
            }).ToListAsync();

            var model = new EditUserViewModel
            {
                ApplicationUser = user,
                ClassList = classList,
                ClassId = user.Id_Class ?? 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.ApplicationUser.Id)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Id_Class = model.ClassId;
            user.Name = model.ApplicationUser.Name;
            user.Surname = model.ApplicationUser.Surname;
            user.Email = model.ApplicationUser.Email;
            user.Patronymic = model.ApplicationUser.Patronymic;
            user.Date_birth = model.ApplicationUser.Date_birth;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception($"Unexpected error occurred setting class for user with ID '{user.Id}'.");
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                // Вывести уведомление об успешном удалении пользователя
                TempData["SuccessMessage"] = "Пользователь успешно удален.";
                return RedirectToAction(nameof(Index));
            }
            // Вывести уведомление об ошибке удаления пользователя
            TempData["ErrorMessage"] = "Ошибка удаления пользователя: " + string.Join(", ", result.Errors.Select(e => e.Description));
            ModelState.AddModelError(string.Empty, "Ошибка при удалении пользователя.");
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return PartialView("Delete", user);
        }
    }
}
