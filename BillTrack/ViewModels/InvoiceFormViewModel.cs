using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BillTrack.ViewModels
{
    public class InvoiceFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Invoice number is required.")]
        [StringLength(30)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        // int? е нужен, за да сработи [Required] при опцията "Select client..."
        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public int? ClientId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
        public decimal? Amount { get; set; }

        // Показва се само при редакция
        [Display(Name = "Invoice is paid")]
        public bool IsPaid { get; set; }

        // Списъкът за падащото меню не идва от формата и не се валидира
        [BindNever]
        [ValidateNever]
        public List<SelectListItem> Clients { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DueDate.Date < IssueDate.Date)
            {
                yield return new ValidationResult(
                    "Due date cannot be earlier than the issue date.",
                    new[] { nameof(DueDate) });
            }
        }
    }
}
