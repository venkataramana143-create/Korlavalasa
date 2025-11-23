using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Korlavalasa.Pages.Admin.Events
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        [BindProperty]
        public Event Event { get; set; } = new Event();

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // No need to set default date here as it's handled by JavaScript
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Server-side validation for future date
            if (Event.EventDate <= DateTime.Now)
            {
                ModelState.AddModelError("Event.EventDate", "Event date must be in the future.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Events.Add(Event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event '{Event.Title}' created successfully!";
            return RedirectToPage("./Index");
        }
    }
}