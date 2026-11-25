using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillTrack.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Invoice number is required.")]
        [StringLength(30)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        public Client? Client { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // В базата пазим само Pending или Paid. Overdue се изчислява.
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Бизнес логика: платена фактура никога не е Overdue,
        // неплатена с изтекъл DueDate е Overdue.
        [NotMapped]
        public InvoiceStatus EffectiveStatus
        {
            get
            {
                if (Status == InvoiceStatus.Paid) return InvoiceStatus.Paid;
                return DueDate.Date < DateTime.Today ? InvoiceStatus.Overdue : InvoiceStatus.Pending;
            }
        }
    }
}
