using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Korlavalasa.Models
{
    public class AdminUser : IdentityUser
    {
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
