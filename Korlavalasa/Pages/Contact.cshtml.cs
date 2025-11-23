using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Korlavalasa.Data;
using Korlavalasa.Models;

namespace Korlavalasa.Pages
{
    public class ContactModel : PageModel
    {
        private readonly AppDbContext _context;
        public VillageInfo VillageInfo { get; set; } = new VillageInfo();

        public ContactModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            VillageInfo = await _context.VillageInfo.FirstOrDefaultAsync() ?? new VillageInfo();
        }
    }
}