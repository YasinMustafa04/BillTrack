using System.ComponentModel.DataAnnotations;

namespace BillTrack.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Company")]
        public string? CompanyName { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(30)]
        public string? Phone { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Един клиент има много фактури (1:N)
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
