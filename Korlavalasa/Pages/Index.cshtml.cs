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
            VillageInfo = await _context.VillageInfo.FirstOrDefaultAsync() ?? new VillageInfo();

            NewsItems = await _context.News
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.PublishedDate)
                .Take(3)
                .ToListAsync();

            UpcomingEvents = await _context.Events
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Take(4)
                .ToListAsync();
        }
    }
}