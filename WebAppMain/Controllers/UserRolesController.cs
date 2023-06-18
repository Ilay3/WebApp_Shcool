using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppMain.Models;

namespace WebAppMain.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string constantFilter, string searchString, string roleFilter, int pageNumber = 1)
        {
            var users = await _userManager.Users.ToListAsync();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => _userManager.IsInRoleAsync(u, roleFilter).Result).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => !string.IsNullOrEmpty(u.Name) && u.Name.Contains(searchString)
                                    || !string.IsNullOrEmpty(u.Surname) && u.Surname.Contains(searchString)
                                    || !string.IsNullOrEmpty(u.Patronymic) && u.Patronymic.Contains(searchString)).ToList();
            }

            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                    Patronymic = user.Patronymic,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                userRolesViewModel.Add(thisViewModel);
            }

            ViewBag.PageNumber = pageNumber;

            return View(userRolesViewModel);
        }


        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} не найден";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            //await _userManager.AddToRoleAsync(user, Enums.Roles.Ученик.ToString());
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Не удается удалить существующие роли пользователей");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Не удается добавить выбранные роли пользователю");
                return View(model);
            }
            return RedirectToAction("Index");
        }

    }
}

