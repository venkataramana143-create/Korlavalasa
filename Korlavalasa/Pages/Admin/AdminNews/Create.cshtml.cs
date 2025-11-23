using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Korlavalasa.Pages.Admin.AdminNews
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        [BindProperty]
        public News News { get; set; } = new News();

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            News.PublishedDate = DateTime.Now;
            _context.News.Add(News);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"News '{News.Title}' created successfully!";
            return RedirectToPage("./Index");
        }
    }
}