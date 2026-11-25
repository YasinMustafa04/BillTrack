using BillTrack.Data;
using BillTrack.Models;
using BillTrack.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillTrack.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Clients?search=ivan
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)));
            }

            var clients = await query
                .OrderBy(c => c.FullName)
                .Select(c => new ClientListItemViewModel
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    CompanyName = c.CompanyName,
                    Phone = c.Phone,
                    InvoiceCount = c.Invoices.Count()
                })
                .ToListAsync();

            return View(new ClientIndexViewModel { Search = search, Clients = clients });
        }

        // GET: /Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // GET: /Clients/Create
        public IActionResult Create()
        {
            return View(new Client());
        }

        // POST: /Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,CompanyName,Phone")] Client client)
        {
            await ValidateUniqueEmailAsync(client);

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            client.CreatedAt = DateTime.UtcNow;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Client \"{client.FullName}\" was created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: /Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,CompanyName,Phone")] Client input)
        {
            if (id != input.Id) return NotFound();

            await ValidateUniqueEmailAsync(input);

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            // Обновяваме само позволените полета (CreatedAt остава непроменено)
            client.FullName = input.FullName;
            client.Email = input.Email;
            client.CompanyName = input.CompanyName;
            client.Phone = input.Phone;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Client \"{client.FullName}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Clients/Delete/5 (страница за потвърждение)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: /Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return RedirectToAction(nameof(Index));

            if (client.Invoices.Any())
            {
                TempData["Error"] = $"\"{client.FullName}\" has invoices and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Client \"{client.FullName}\" was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Проверка за уникален имейл (без значение от малки/главни букви)
        private async Task ValidateUniqueEmailAsync(Client client)
        {
            var email = client.Email?.Trim().ToLower();
            if (string.IsNullOrEmpty(email)) return;

            var exists = await _context.Clients
                .AnyAsync(c => c.Id != client.Id && c.Email.ToLower() == email);

            if (exists)
            {
                ModelState.AddModelError(nameof(Client.Email), "A client with this email already exists.");
            }
        }
    }
}
