using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Korlavalasa.Data;
using Korlavalasa.Models;

namespace Korlavalasa.Pages.Events
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public List<Event> UpcomingEvents { get; set; } = new List<Event>();
        public List<Event> PastEvents { get; set; } = new List<Event>();

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
                .Take(10)
                .ToListAsync();
        }
    }
}