using BillTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace BillTrack.Data
{
    // Създава примерни данни при първо стартиране (само ако базата е празна).
    // Датите са спрямо днешния ден, затова Dashboard-ът изглежда добре във всеки ден.
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Clients.AnyAsync() || await context.Invoices.AnyAsync())
            {
                return;
            }

            var today = DateTime.Today;
            var now = DateTime.UtcNow;

            var ivan = new Client { FullName = "Ivan Petrov", Email = "ivan@petrovstudio.example.com", CompanyName = "Petrov Studio", Phone = "+359888111111", CreatedAt = now.AddDays(-90) };
            var maria = new Client { FullName = "Maria Georgieva", Email = "maria@georgievadesign.example.com", CompanyName = "Georgieva Design", Phone = "+359888222222", CreatedAt = now.AddDays(-75) };
            var georgi = new Client { FullName = "Georgi Dimitrov", Email = "georgi@dimitrovtech.example.com", CompanyName = "Dimitrov Tech", Phone = "+359888333333", CreatedAt = now.AddDays(-60) };
            var elena = new Client { FullName = "Elena Stoyanova", Email = "elena@stoyanova.example.com", CompanyName = "Stoyanova Consulting", Phone = "+359888444444", CreatedAt = now.AddDays(-45) };
            var nikolay = new Client { FullName = "Nikolay Ivanov", Email = "nikolay@ivanovtrade.example.com", CompanyName = "Ivanov Trade", Phone = "+359888555555", CreatedAt = now.AddDays(-30) };

            context.Clients.AddRange(ivan, maria, georgi, elena, nikolay);

            // 4 платени, 3 чакащи, 3 просрочени (Overdue = неплатена с изтекъл падеж)
            context.Invoices.AddRange(
                NewInvoice("INV-2026-001", ivan, "Website redesign", today.AddDays(-70), today.AddDays(-56), 2400.00m, InvoiceStatus.Paid),
                NewInvoice("INV-2026-002", ivan, "Monthly hosting and maintenance", today.AddDays(-50), today.AddDays(-36), 350.00m, InvoiceStatus.Paid),
                NewInvoice("INV-2026-003", maria, "Logo and brand identity", today.AddDays(-45), today.AddDays(-30), 1200.00m, InvoiceStatus.Pending),
                NewInvoice("INV-2026-004", maria, "Social media campaign", today.AddDays(-10), today.AddDays(20), 1800.00m, InvoiceStatus.Pending),
                NewInvoice("INV-2026-005", georgi, "Mobile app prototype", today.AddDays(-65), today.AddDays(-50), 5200.00m, InvoiceStatus.Paid),
                NewInvoice("INV-2026-006", georgi, "UX audit", today.AddDays(-35), today.AddDays(-21), 950.00m, InvoiceStatus.Pending),
                NewInvoice("INV-2026-007", elena, "SEO optimization package", today.AddDays(-5), today.AddDays(25), 780.00m, InvoiceStatus.Pending),
                NewInvoice("INV-2026-008", elena, "E-commerce integration", today.AddDays(-80), today.AddDays(-64), 3100.00m, InvoiceStatus.Paid),
                NewInvoice("INV-2026-009", nikolay, "Consulting hours (20h)", today.AddDays(-2), today.AddDays(28), 1500.00m, InvoiceStatus.Pending),
                NewInvoice("INV-2026-010", nikolay, "Content writing", today.AddDays(-28), today.AddDays(-14), 640.00m, InvoiceStatus.Pending)
            );

            await context.SaveChangesAsync();
        }

        private static Invoice NewInvoice(string number, Client client, string description,
            DateTime issueDate, DateTime dueDate, decimal amount, InvoiceStatus status)
        {
            return new Invoice
            {
                InvoiceNumber = number,
                Client = client,
                Description = description,
                IssueDate = issueDate,
                DueDate = dueDate,
                Amount = amount,
                Status = status,
                CreatedAt = issueDate.ToUniversalTime()
            };
        }
    }
}
