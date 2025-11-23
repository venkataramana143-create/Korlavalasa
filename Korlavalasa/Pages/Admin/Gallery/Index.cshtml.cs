using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Korlavalasa.Pages.Admin.Gallery
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;

        public List<GalleryImage> GalleryImages { get; set; } = new List<GalleryImage>();

        public IndexModel(AppDbContext context, IWebHostEnvironment environment, ILogger<IndexModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                GalleryImages = await _context.Gallery
                    .OrderByDescending(g => g.UploadDate)
                    .ToListAsync();

                _logger.LogInformation($"Loaded {GalleryImages.Count} gallery images");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading gallery images");
                TempData["ErrorMessage"] = "Error loading gallery images. Please try again.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var image = await _context.Gallery.FindAsync(id);
                if (image == null)
                {
                    TempData["ErrorMessage"] = "Image not found.";
                    return RedirectToPage();
                }

                // Delete physical file using IWebHostEnvironment for correct path
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

                TempData["SuccessMessage"] = $"Image '{image.Title}' deleted successfully!";
                _logger.LogInformation($"Image '{image.Title}' (ID: {image.Id}) deleted successfully");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while deleting image");
                TempData["ErrorMessage"] = "Database error occurred while deleting the image.";
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "File system error while deleting image");
                TempData["ErrorMessage"] = "Error deleting image file. The database record was removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting image");
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the image.";
            }

            return RedirectToPage();
        }
    }
}