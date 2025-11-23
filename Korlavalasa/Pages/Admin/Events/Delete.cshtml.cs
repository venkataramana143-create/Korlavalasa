using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.Events
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public Event Event { get; set; } = new();

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Event = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);

            if (Event == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Event '{eventItem.Title}' deleted successfully!";
            }

            return RedirectToPage("./Index");
        }
    }
}