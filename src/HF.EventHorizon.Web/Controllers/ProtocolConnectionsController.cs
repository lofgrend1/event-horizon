using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using HF.EventHorizon.Core.Entities;
using HF.EventHorizon.Infrastructure.Data;

namespace HF.EventHorizon.Web.Controllers;

public class ProtocolConnectionsController : Controller
{
    private readonly EvtHorizonContext _context;

    public ProtocolConnectionsController(EvtHorizonContext context)
    {
        _context = context;
    }

    // GET: ProtocolConnections
    public async Task<IActionResult> Index()
    {
        var evtHorizonContext = _context.ProtocolConnections.Include(p => p.ProtocolPlugin);
        return View(await evtHorizonContext.ToListAsync());
    }

    // GET: ProtocolConnections/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var protocolConnection = await _context.ProtocolConnections
            .Include(p => p.ProtocolPlugin)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (protocolConnection == null)
        {
            return NotFound();
        }

        return View(protocolConnection);
    }

    // GET: ProtocolConnections/Create
    public IActionResult Create()
    {
        ViewData["ProtocolPluginId"] = new SelectList(_context.ProtocolPlugins, "Id", "Name");
        return View();
    }

    // POST: ProtocolConnections/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ProtocolPluginId,ConnectionString,AdditionalParametersJson,Id,CreatedAt,UpdatedAt")] ProtocolConnection protocolConnection)
    {
        if (ModelState.IsValid)
        {
            _context.Add(protocolConnection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["ProtocolPluginId"] = new SelectList(_context.ProtocolPlugins, "Id", "Name", protocolConnection.ProtocolPluginId);
        return View(protocolConnection);
    }

    // GET: ProtocolConnections/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var protocolConnection = await _context.ProtocolConnections.FindAsync(id);
        if (protocolConnection == null)
        {
            return NotFound();
        }
        ViewData["ProtocolPluginId"] = new SelectList(_context.ProtocolPlugins, "Id", "Name", protocolConnection.ProtocolPluginId);
        return View(protocolConnection);
    }

    // POST: ProtocolConnections/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Name,ProtocolPluginId,ConnectionString,AdditionalParametersJson,Id,CreatedAt,UpdatedAt")] ProtocolConnection protocolConnection)
    {
        if (id != protocolConnection.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(protocolConnection);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProtocolConnectionExists(protocolConnection.Id))
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
        ViewData["ProtocolPluginId"] = new SelectList(_context.ProtocolPlugins, "Id", "Name", protocolConnection.ProtocolPluginId);
        return View(protocolConnection);
    }

    // GET: ProtocolConnections/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var protocolConnection = await _context.ProtocolConnections
            .Include(p => p.ProtocolPlugin)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (protocolConnection == null)
        {
            return NotFound();
        }

        return View(protocolConnection);
    }

    // POST: ProtocolConnections/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var protocolConnection = await _context.ProtocolConnections.FindAsync(id);
        if (protocolConnection != null)
        {
            _context.ProtocolConnections.Remove(protocolConnection);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProtocolConnectionExists(int id)
    {
        return _context.ProtocolConnections.Any(e => e.Id == id);
    }
}
