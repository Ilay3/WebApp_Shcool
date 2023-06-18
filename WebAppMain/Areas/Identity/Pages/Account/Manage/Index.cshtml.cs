using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppMain.Models;

namespace WebAppMain.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Имя")]
            public string Name { get; set; }
            [Display(Name = "Фаилия")]
            public string Surname { get; set; }
            [Display(Name = "Отчество")]
            public string Patronymic { get; set; }
            [Display(Name = "Дата рождения")]
            public DateTime Date_birth { get; set; }
            [Phone]
            [Display(Name = "Номер телефона")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Фото профиля")]
            public byte[] ProfilePicture { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var profilePicture = user.ProfilePicture;
            var name = user.Name;
            var surname = user.Surname;
            var patronymic = user.Patronymic;
            var data_birth = user.Date_birth;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ProfilePicture = profilePicture,
                Name = name,
                Surname = surname,
                Date_birth = data_birth,
                Patronymic = patronymic
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            var name = user.Name;
            var surname = user.Surname;
            var patronymic = user.Patronymic;
            var data_birth = user.Date_birth;

            if (Input.Name != name)
            {
                user.Name = Input.Name;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Surname != surname)
            {
                user.Surname = Input.Surname;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Patronymic != patronymic)
            {
                user.Patronymic = Input.Patronymic;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Date_birth != data_birth)
            {
                user.Date_birth = Input.Date_birth;
                await _userManager.UpdateAsync(user);
            }

            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    user.ProfilePicture = dataStream.ToArray();
                }
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Ваш профиль был обновлен";
            return RedirectToPage();
        }
    }
}
