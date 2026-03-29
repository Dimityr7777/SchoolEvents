using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolEvents.Models;

namespace SchoolEvents.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Полето Имейл е задължително.")]
            [EmailAddress(ErrorMessage = "Въведи валиден имейл.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Полето Парола е задължително.")]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне 6 символа.")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Полето Потвърди парола е задължително.")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Избери роля.")]
            public string Role { get; set; } = "Student";

            public bool WantsEmailNotifications { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Input.Role != "Student" && Input.Role != "Teacher")
            {
                ModelState.AddModelError(string.Empty, "Невалидна роля.");
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                WantsEmailNotifications = Input.WantsEmailNotifications,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, Input.Role);

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    await _userManager.DeleteAsync(user);
                    return Page();
                }

                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _signInManager.RefreshSignInAsync(user);

                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}