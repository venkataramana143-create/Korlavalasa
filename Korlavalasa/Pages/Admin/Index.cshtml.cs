using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public int NewsCount { get; set; }
        public int EventCount { get; set; }
        public int GalleryCount { get; set; }
        public VillageInfo VillageInfo { get; set; } = new VillageInfo();

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            NewsCount = await _context.News.CountAsync();
            EventCount = await _context.Events.CountAsync();
            GalleryCount = await _context.Gallery.CountAsync();
            VillageInfo = await _context.VillageInfo.FirstOrDefaultAsync() ?? new VillageInfo();
        }
    }
}