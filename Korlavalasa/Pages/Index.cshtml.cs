using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Korlavalasa.Data;
using Korlavalasa.Models;

namespace Korlavalasa.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public VillageInfo VillageInfo { get; set; } = new VillageInfo();
        public List<News> NewsItems { get; set; } = new List<News>();
        public List<Event> UpcomingEvents { get; set; } = new List<Event>();
        public int MainCropsCount => VillageInfo.MainCrops?.Split(',').Length ?? 0;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Village Info
            VillageInfo = await _context.VillageInfo.FirstOrDefaultAsync() ?? new VillageInfo();

            // FIX: Convert PublishedDate to UTC to match Postgres timestamp with time zone
            NewsItems = await _context.News
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.PublishedDate.ToUniversalTime())
                .Take(3)
                .ToListAsync();

            // FIX: Convert EventDate also to UTC for comparison
            UpcomingEvents = await _context.Events
                .Where(e => e.EventDate.ToUniversalTime() >= DateTime.UtcNow.Date)
                .OrderBy(e => e.EventDate.ToUniversalTime())
                .Take(4)
                .ToListAsync();
        }
    }
}
