using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using HF.EventHorizon.Core.Entities;
using HF.EventHorizon.Infrastructure.Data;

namespace HF.EventHorizon.Web.Controllers
{
    public class BrowsedAddressesController : Controller
    {
        private readonly EvtHorizonContext _context;

        public BrowsedAddressesController(EvtHorizonContext context)
        {
            _context = context;
        }

        // GET: BrowsedAddresses
        public async Task<IActionResult> Index()
        {
            var evtHorizonContext = _context.BrowsedAddresses.Include(b => b.ProtocolConnection);
            return View(await evtHorizonContext.ToListAsync());
        }

        // GET: BrowsedAddresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var browsedAddress = await _context.BrowsedAddresses
                .Include(b => b.ProtocolConnection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (browsedAddress == null)
            {
                return NotFound();
            }

            return View(browsedAddress);
        }

        // GET: BrowsedAddresses/Create
        public IActionResult Create()
        {
            ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson");
            return View();
        }

        // POST: BrowsedAddresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProtocolConnectionId,Address,Id,CreatedAt,UpdatedAt")] BrowsedAddress browsedAddress)
        {
            if (ModelState.IsValid)
            {
                _context.Add(browsedAddress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", browsedAddress.ProtocolConnectionId);
            return View(browsedAddress);
        }

        // GET: BrowsedAddresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var browsedAddress = await _context.BrowsedAddresses.FindAsync(id);
            if (browsedAddress == null)
            {
                return NotFound();
            }
            ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", browsedAddress.ProtocolConnectionId);
            return View(browsedAddress);
        }

        // POST: BrowsedAddresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProtocolConnectionId,Address,Id,CreatedAt,UpdatedAt")] BrowsedAddress browsedAddress)
        {
            if (id != browsedAddress.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(browsedAddress);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrowsedAddressExists(browsedAddress.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", browsedAddress.ProtocolConnectionId);
            return View(browsedAddress);
        }

        // GET: BrowsedAddresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var browsedAddress = await _context.BrowsedAddresses
                .Include(b => b.ProtocolConnection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (browsedAddress == null)
            {
                return NotFound();
            }

            return View(browsedAddress);
        }

        // POST: BrowsedAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var browsedAddress = await _context.BrowsedAddresses.FindAsync(id);
            if (browsedAddress != null)
            {
                _context.BrowsedAddresses.Remove(browsedAddress);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BrowsedAddressExists(int id)
        {
            return _context.BrowsedAddresses.Any(e => e.Id == id);
        }
    }
}
