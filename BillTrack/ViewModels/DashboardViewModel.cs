using BillTrack.Models;

namespace BillTrack.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalInvoices { get; set; }
        public decimal TotalRevenue { get; set; }

        public int PaidCount { get; set; }
        public decimal PaidAmount { get; set; }

        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }

        public int OverdueCount { get; set; }
        public decimal OverdueAmount { get; set; }

        public List<Invoice> RecentInvoices { get; set; } = new();
    }
}
