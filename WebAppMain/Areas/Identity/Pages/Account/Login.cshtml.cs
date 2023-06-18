using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using WebAppMain.Models;
using System.Net.Mail;
using Newtonsoft.Json;

namespace WebAppMain.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]

            public string Email { get; set; }

            [Display(Name = "Пароль")]
            [Required(ErrorMessage = "ведите пароль")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Запомнить?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }
        //проверка, являются ли введенные данные действительным идентификатором электронной почты или нет
        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "secret", "6Ld83FslAAAAAHcY0WG_e-9YMC59ZcvTcAFiLmTz" },
                        { "response", Request.Form["g-recaptcha-response"] }
                    }));

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<RecaptchaResponse>(json);

                        if (result.Success) // Проверка успешного прохождения ReCAPTCHA
                        {
                            var userName = Input.Email;
                            if (IsValidEmail(Input.Email))
                            {
                                var user = await _userManager.FindByEmailAsync(Input.Email);
                                if (user != null)
                                {
                                    userName = user.UserName;
                                }
                            }

                            var signInResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                            if (!signInResult.Succeeded && userName != Input.Email)
                            {
                                signInResult = await _signInManager.PasswordSignInAsync(userName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                            }
                            if (signInResult.Succeeded)
                            {
                                _logger.LogInformation("User logged in.");
                                return LocalRedirect(returnUrl);
                            }
                            if (signInResult.RequiresTwoFactor)
                            {
                                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                            }
                            if (signInResult.IsLockedOut)
                            {
                                _logger.LogWarning("User account locked out.");
                                return RedirectToPage("./Lockout");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Не вверно введены данные.");
                                return Page();
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Не удалось пройти проверку reCAPTCHA.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Не удалось установить связь с сервером проверки reCAPTCHA.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}