using System.ComponentModel.DataAnnotations;

namespace Korlavalasa.Models
{
    public class VillageInfo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "About text is required")]
        [Display(Name = "About Village")]
        public string AboutText { get; set; } = string.Empty;

        [Required(ErrorMessage = "History is required")]
        public string History { get; set; } = string.Empty;

        [Required(ErrorMessage = "Population is required")]
        [Range(1, 1000000, ErrorMessage = "Population must be between 1 and 1,000,000")]
        public int Population { get; set; }

        [Required(ErrorMessage = "Area is required")]
        [Range(0.01, 10000, ErrorMessage = "Area must be between 0.01 and 10,000")]
        [Display(Name = "Area (sq km)")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Main crops information is required")]
        [Display(Name = "Main Crops")]
        public string MainCrops { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sarpanch name is required")]
        [Display(Name = "Sarpanch Name")]
        public string SarpanchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Secretary name is required")]
        [Display(Name = "Secretary Name")]
        public string SecretaryName { get; set; } = string.Empty;
    }
}