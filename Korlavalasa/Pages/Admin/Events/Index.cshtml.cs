using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.Events
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public List<Event> UpcomingEvents { get; set; } = new();
        public List<Event> PastEvents { get; set; } = new();

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;

            UpcomingEvents = await _context.Events
                .Where(e => e.EventDate >= today)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            PastEvents = await _context.Events
                .Where(e => e.EventDate < today)
                .OrderByDescending(e => e.EventDate)
                .Take(50) // Limit past events to last 50
                .ToListAsync();
        }
    }
}