using BillTrack.Models;

namespace BillTrack.ViewModels
{
    public class InvoiceIndexViewModel
    {
        public string? Search { get; set; }
        public InvoiceStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Sort { get; set; } = "date_desc";

        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }

        public List<Invoice> Invoices { get; set; } = new();

        public bool HasFilters =>
            !string.IsNullOrWhiteSpace(Search) || Status.HasValue || DateFrom.HasValue || DateTo.HasValue;
    }
}
