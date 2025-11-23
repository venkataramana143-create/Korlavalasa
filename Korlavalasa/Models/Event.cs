using System.ComponentModel.DataAnnotations;

namespace Korlavalasa.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? ContactNumber { get; set; }
    }
}