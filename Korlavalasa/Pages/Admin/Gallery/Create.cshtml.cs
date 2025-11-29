using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Korlavalasa.Pages.Admin.Gallery
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CreateModel> _logger;
        private readonly Cloudinary _cloudinary;

        [BindProperty]
        public GalleryImage GalleryImage { get; set; } = new GalleryImage();

        public CreateModel(
            AppDbContext context,
            ILogger<CreateModel> logger,
            Cloudinary cloudinary)
        {
            _context = context;
            _logger = logger;
            _cloudinary = cloudinary;
        }

        public IActionResult OnGet()
        {
            GalleryImage.Category = "General";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("=== MULTIPLE IMAGE UPLOAD STARTED ===");

                // Get uploaded files
                var formFiles = Request.Form.Files;
                if (formFiles.Count == 0)
                {
                    ViewData["Error"] = "Please select at least one image.";
                    return Page();
                }

                // Validate category
                if (string.IsNullOrWhiteSpace(GalleryImage.Category))
                {
                    ViewData["Error"] = "Category is required.";
                    return Page();
                }

                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var maxSize = 5 * 1024 * 1024; // 5 MB

                var uploadedImages = new List<GalleryImage>();
                int successCount = 0;

                foreach (var file in formFiles)
                {
                    if (file == null || file.Length == 0)
                        continue;

                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!validExtensions.Contains(ext))
                    {
                        _logger.LogWarning($"Invalid file skipped: {file.FileName}");
                        continue;
                    }

                    if (file.Length > maxSize)
                    {
                        _logger.LogWarning($"File too large skipped: {file.FileName}");
                        continue;
                    }

                    // ------------------------
                    // CLOUDINARY UPLOAD
                    // ------------------------
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = "korlavalasa/gallery"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult == null ||
                        uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Cloudinary failed for: {file.FileName}");
                        continue;
                    }

                    string cloudUrl = uploadResult.SecureUrl.ToString();
                    _logger.LogInformation($"Uploaded: {file.FileName} → {cloudUrl}");

                    // Save to DB entity
                    var img = new GalleryImage
                    {
                        Title = Path.GetFileNameWithoutExtension(file.FileName),
                        Category = GalleryImage.Category,
                        Description = "",
                        UploadDate = DateTime.UtcNow,
                        ImagePath = cloudUrl   // IMPORTANT
                    };

                    uploadedImages.Add(img);
                    successCount++;
                }

                // Save database entries
                if (uploadedImages.Any())
                {
                    _context.Gallery.AddRange(uploadedImages);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Uploaded {successCount} image(s) successfully!";
                    _logger.LogInformation("=== UPLOAD COMPLETE ===");

                    return RedirectToPage("./Index");
                }
                else
                {
                    ViewData["Error"] = "No valid images uploaded.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL ERROR during upload");
                ViewData["Error"] = "A critical error occurred. Please try again.";
                return Page();
            }
        }
    }
}
