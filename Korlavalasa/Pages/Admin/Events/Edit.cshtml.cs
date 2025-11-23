using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.Events
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        [BindProperty]
        public Event Event { get; set; } = new();

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            Event = eventItem;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(Event.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = $"Event '{Event.Title}' updated successfully!";
            return RedirectToPage("./Index");
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}