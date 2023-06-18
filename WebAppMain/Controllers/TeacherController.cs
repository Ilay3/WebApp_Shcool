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
    public class TeacherController : Controller
    {
        private ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TeacherController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = applicationDbContext;
        }


        public async Task<IActionResult> IndexAsync(string searchString, int? page, string currentFilter, string className)
        {
            var classes = db.Class.ToList();
            var roles = _roleManager.Roles.ToList();
            var users = _userManager.Users.ToList();

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            //фильтруем пользователей по имени или фамилии
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => !string.IsNullOrEmpty(u.Name) && u.Name.Contains(searchString)
                                    || !string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(searchString)).ToList();
            }
            // фильтруем пользователей по классу
            if (!string.IsNullOrEmpty(className))
            {
                if (className == "NoClass") // проверка на значение "NoClass"
                {
                    users = users.Where(u => u.Class == null).ToList(); // фильтрация пользователей у которых класс равен null
                }
                else
                {
                    users = users.Where(u => u.Class != null && u.Class.Title_Class != null && u.Class.Title_Class.Contains(className)).ToList();
                }
            }

            //фильтруем пользователей по роли 
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Учитель");
            if (role != null)
            {
                users = users.Where(u => _userManager.IsInRoleAsync(u, role.Name).Result).ToList();
            }
            //устанавливаем размер страницы и получаем количество страниц
            int pageSize = 7;
            int pageNumber = (page ?? 1);
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
