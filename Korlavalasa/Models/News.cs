using System.ComponentModel.DataAnnotations;

namespace Korlavalasa.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Image")]
        [Url(ErrorMessage = "Please enter a valid image URL")]
        public string? ImageUrl { get; set; }
    }
}