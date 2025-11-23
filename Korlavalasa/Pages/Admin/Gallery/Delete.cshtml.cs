using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.Gallery
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(AppDbContext context, IWebHostEnvironment environment, ILogger<DeleteModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [BindProperty]
        public GalleryImage GalleryImage { get; set; } = new GalleryImage();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Gallery.FirstOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                return NotFound();
            }

            GalleryImage = image;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (GalleryImage?.Id == 0)
            {
                return NotFound();
            }

            try
            {
                var image = await _context.Gallery.FindAsync(GalleryImage.Id);
                if (image == null)
                {
                    return NotFound();
                }

                // Delete physical file
                if (!string.IsNullOrEmpty(image.ImagePath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, image.ImagePath.TrimStart('/'));
                    _logger.LogInformation($"Attempting to delete file: {filePath}");

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation($"File deleted successfully: {filePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"File not found at path: {filePath}");
                    }
                }

                // Remove from database
                _context.Gallery.Remove(image);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Image '{image.Title}' has been deleted successfully!";
                _logger.LogInformation($"Image '{image.Title}' (ID: {image.Id}) deleted successfully");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while deleting image");
                TempData["ErrorMessage"] = "Database error occurred while deleting the image.";
                return Page();
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "File system error while deleting image");
                TempData["ErrorMessage"] = "Error deleting image file. The database record was removed.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting image");
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the image.";
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}