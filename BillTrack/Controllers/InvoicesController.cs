using BillTrack.Data;
using BillTrack.Models;
using BillTrack.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BillTrack.Controllers
{
    public class InvoicesController : Controller
    {
        private const int PageSize = 8;

        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Invoices?search=&status=&dateFrom=&dateTo=&sort=&page=
        public async Task<IActionResult> Index(
            string? search,
            InvoiceStatus? status,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? sort,
            int page = 1)
        {
            var today = DateTime.Today;
            var query = _context.Invoices.AsQueryable();

            // Търсене по номер на фактура или име на клиент
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(i =>
                    i.InvoiceNumber.ToLower().Contains(term) ||
                    i.Client!.FullName.ToLower().Contains(term));
            }

            // Филтър по статус. Overdue се определя по DueDate, не по записания статус.
            if (status.HasValue)
            {
                query = status.Value switch
                {
                    InvoiceStatus.Paid => query.Where(i => i.Status == InvoiceStatus.Paid),
                    InvoiceStatus.Overdue => query.Where(i => i.Status != InvoiceStatus.Paid && i.DueDate < today),
                    _ => query.Where(i => i.Status != InvoiceStatus.Paid && i.DueDate >= today)
                };
            }

            // Филтър по дата на издаване (включително крайната дата)
            if (dateFrom.HasValue)
            {
                var start = dateFrom.Value.Date;
                query = query.Where(i => i.IssueDate >= start);
            }
            if (dateTo.HasValue)
            {
                var endExclusive = dateTo.Value.Date.AddDays(1);
                query = query.Where(i => i.IssueDate < endExclusive);
            }

            // Сортиране
            sort = sort is "date_asc" or "due_asc" ? sort : "date_desc";
            query = sort switch
            {
                "date_asc" => query.OrderBy(i => i.IssueDate).ThenBy(i => i.Id),
                "due_asc" => query.OrderBy(i => i.DueDate).ThenBy(i => i.Id),
                _ => query.OrderByDescending(i => i.IssueDate).ThenByDescending(i => i.Id)
            };

            // Пагинация
            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var invoices = await query
                .Include(i => i.Client)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new InvoiceIndexViewModel
            {
                Search = search,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Sort = sort,
                Page = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Invoices = invoices
            };

            return View(viewModel);
        }

        // GET: /Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // GET: /Invoices/Create
        public async Task<IActionResult> Create(int? clientId)
        {
            var model = new InvoiceFormViewModel
            {
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                ClientId = clientId,
                Clients = await GetClientOptionsAsync()
            };

            return View(model);
        }

        // POST: /Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceFormViewModel model)
        {
            await ValidateInvoiceAsync(model);

            if (!ModelState.IsValid)
            {
                model.Clients = await GetClientOptionsAsync();
                return View(model);
            }

            var invoice = new Invoice
            {
                InvoiceNumber = model.InvoiceNumber.Trim(),
                ClientId = model.ClientId!.Value,
                Description = model.Description.Trim(),
                IssueDate = model.IssueDate,
                DueDate = model.DueDate,
                Amount = model.Amount!.Value,
                Status = InvoiceStatus.Pending, // по подразбиране винаги Pending
                CreatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Друга заявка е записала същия номер между проверката и записа (напр. двоен клик)
                ModelState.AddModelError(nameof(model.InvoiceNumber), "An invoice with this number already exists.");
                model.Clients = await GetClientOptionsAsync();
                return View(model);
            }

            TempData["Success"] = $"Invoice {invoice.InvoiceNumber} was created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            var model = new InvoiceFormViewModel
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                ClientId = invoice.ClientId,
                Description = invoice.Description,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Amount = invoice.Amount,
                IsPaid = invoice.Status == InvoiceStatus.Paid,
                Clients = await GetClientOptionsAsync()
            };

            return View(model);
        }

        // POST: /Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvoiceFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            await ValidateInvoiceAsync(model);

            if (!ModelState.IsValid)
            {
                model.Clients = await GetClientOptionsAsync();
                return View(model);
            }

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            invoice.InvoiceNumber = model.InvoiceNumber.Trim();
            invoice.ClientId = model.ClientId!.Value;
            invoice.Description = model.Description.Trim();
            invoice.IssueDate = model.IssueDate;
            invoice.DueDate = model.DueDate;
            invoice.Amount = model.Amount!.Value;
            invoice.Status = model.IsPaid ? InvoiceStatus.Paid : InvoiceStatus.Pending;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                ModelState.AddModelError(nameof(model.InvoiceNumber), "An invoice with this number already exists.");
                model.Clients = await GetClientOptionsAsync();
                return View(model);
            }

            TempData["Success"] = $"Invoice {invoice.InvoiceNumber} was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Invoices/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id, string? returnUrl)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            if (invoice.Status != InvoiceStatus.Paid)
            {
                invoice.Status = InvoiceStatus.Paid;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Invoice {invoice.InvoiceNumber} was marked as paid.";
            }

            // Връщаме потребителя там, откъдето е дошъл (само към адреси от нашия сайт)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Invoices/Delete/5 (страница за потвърждение)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // POST: /Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Invoice {invoice.InvoiceNumber} was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------- Помощни методи ----------

        // Следващ номер във формат INV-{година}-{пореден номер}
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var prefix = $"INV-{DateTime.Today.Year}-";

            var numbers = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .Select(i => i.InvoiceNumber)
                .ToListAsync();

            var max = numbers
                .Select(n => int.TryParse(n.Substring(prefix.Length), out var value) ? value : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{max + 1:000}";
        }

        private async Task<List<SelectListItem>> GetClientOptionsAsync()
        {
            var clients = await _context.Clients
                .OrderBy(c => c.FullName)
                .Select(c => new { c.Id, c.FullName, c.CompanyName })
                .ToListAsync();

            return clients
                .Select(c => new SelectListItem(
                    string.IsNullOrEmpty(c.CompanyName) ? c.FullName : $"{c.FullName} ({c.CompanyName})",
                    c.Id.ToString()))
                .ToList();
        }

        // Проверки, които Data Annotations не могат да направят (изискват заявка към базата)
        private async Task ValidateInvoiceAsync(InvoiceFormViewModel model)
        {
            var number = model.InvoiceNumber?.Trim();
            if (!string.IsNullOrEmpty(number))
            {
                var exists = await _context.Invoices
                    .AnyAsync(i => i.Id != model.Id && i.InvoiceNumber == number);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.InvoiceNumber), "An invoice with this number already exists.");
                }
            }

            if (model.ClientId.HasValue)
            {
                var clientExists = await _context.Clients.AnyAsync(c => c.Id == model.ClientId.Value);
                if (!clientExists)
                {
                    ModelState.AddModelError(nameof(model.ClientId), "The selected client does not exist.");
                }
            }
        }

        // SQLite код 19 = нарушено ограничение (тук: UNIQUE индекс)
        private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqliteException sqliteEx &&
            sqliteEx.SqliteErrorCode == 19 &&
            sqliteEx.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
    }
}
