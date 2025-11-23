using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.AdminNews
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public News News { get; set; } = new News();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            News = news;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (News.Id == 0)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(News.Id);
            if (news == null)
            {
                return NotFound();
            }

            string newsTitle = news.Title;

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"News '{newsTitle}' has been deleted successfully!";
            return RedirectToPage("./Index");
        }
    }
}