using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.NewsUI
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public List<News> NewsItems { get; set; } = new List<News>();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var newsQuery = _context.News.Where(n => n.IsActive);

            if (!string.IsNullOrEmpty(SearchString))
            {
                newsQuery = newsQuery.Where(n =>
                    n.Title.Contains(SearchString) ||
                    n.Content.Contains(SearchString));
            }

            NewsItems = await newsQuery
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();
        }
    }
}