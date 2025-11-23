using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Korlavalasa.Models;

namespace Korlavalasa.Pages.Admin
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<AdminUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<AdminUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET handler - shows the confirmation page
        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // If user is not logged in, redirect to home
                return RedirectToPage("/Index");
            }
            return Page();
        }

        // POST handler - performs the actual logout
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var userName = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserName} logged out.", userName);

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
    }
}