using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.AdminNews
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public List<News> NewsItems { get; set; } = new List<News>();

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            NewsItems = await _context.News
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();
        }
    }
}