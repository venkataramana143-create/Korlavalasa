using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class VillageInfoModel : PageModel
    {
        private readonly AppDbContext _context;

        [BindProperty]
        public VillageInfo VillageInfo { get; set; } = new VillageInfo();

        public VillageInfoModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var villageInfo = await _context.VillageInfo.FirstOrDefaultAsync();
            if (villageInfo == null)
            {
                // Create default if not exists
                villageInfo = new VillageInfo();
                _context.VillageInfo.Add(villageInfo);
                await _context.SaveChangesAsync();
            }

            VillageInfo = villageInfo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(VillageInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Village information updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VillageInfoExists(VillageInfo.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Admin/Index");
        }

        private bool VillageInfoExists(int id)
        {
            return _context.VillageInfo.Any(e => e.Id == id);
        }
    }
}