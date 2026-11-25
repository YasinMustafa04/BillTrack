using BillTrack.Data;
using BillTrack.Models;
using BillTrack.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillTrack.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // Заявките се описват с LINQ, но се изпълняват в базата чак при await
            var paid = _context.Invoices.Where(i => i.Status == InvoiceStatus.Paid);
            var pending = _context.Invoices.Where(i => i.Status != InvoiceStatus.Paid && i.DueDate >= today);
            var overdue = _context.Invoices.Where(i => i.Status != InvoiceStatus.Paid && i.DueDate < today);

            var viewModel = new DashboardViewModel
            {
                TotalInvoices = await _context.Invoices.CountAsync(),
                TotalRevenue = await SumAmountAsync(_context.Invoices),

                PaidCount = await paid.CountAsync(),
                PaidAmount = await SumAmountAsync(paid),

                PendingCount = await pending.CountAsync(),
                PendingAmount = await SumAmountAsync(pending),

                OverdueCount = await overdue.CountAsync(),
                OverdueAmount = await SumAmountAsync(overdue),

                RecentInvoices = await _context.Invoices
                    .Include(i => i.Client)
                    .OrderByDescending(i => i.IssueDate)
                    .ThenByDescending(i => i.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // SQLite не поддържа Sum върху decimal, затова сумираме като double и закръгляме
        private static async Task<decimal> SumAmountAsync(IQueryable<Invoice> query)
        {
            var total = await query.SumAsync(i => (double)i.Amount);
            return Math.Round((decimal)total, 2);
        }
    }
}
