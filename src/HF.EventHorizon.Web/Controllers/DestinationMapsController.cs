using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using HF.EventHorizon.Core.Entities;
using HF.EventHorizon.Infrastructure.Data;

namespace HF.EventHorizon.Web.Controllers;

public class DestinationMapsController : Controller
{
    private readonly EvtHorizonContext _context;

    public DestinationMapsController(EvtHorizonContext context)
    {
        _context = context;
    }

    // GET: DestinationMaps
    public async Task<IActionResult> Index()
    {
        var evtHorizonContext = _context.DestinationMaps.Include(d => d.ProtocolConnection).Include(d => d.RoutingRule);
        return View(await evtHorizonContext.ToListAsync());
    }

    // GET: DestinationMaps/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var destinationMap = await _context.DestinationMaps
            .Include(d => d.ProtocolConnection)
            .Include(d => d.RoutingRule)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (destinationMap == null)
        {
            return NotFound();
        }

        return View(destinationMap);
    }

    // GET: DestinationMaps/Create
    public IActionResult Create()
    {
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson");
        ViewData["RoutingRuleId"] = new SelectList(_context.RoutingRules, "Id", "Address");
        return View();
    }

    // POST: DestinationMaps/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProtocolConnectionId,RoutingRuleId,Address,Id,CreatedAt,UpdatedAt")] DestinationMap destinationMap)
    {
        if (ModelState.IsValid)
        {
            _context.Add(destinationMap);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", destinationMap.ProtocolConnectionId);
        ViewData["RoutingRuleId"] = new SelectList(_context.RoutingRules, "Id", "Address", destinationMap.RoutingRuleId);
        return View(destinationMap);
    }

    // GET: DestinationMaps/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var destinationMap = await _context.DestinationMaps.FindAsync(id);
        if (destinationMap == null)
        {
            return NotFound();
        }
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", destinationMap.ProtocolConnectionId);
        ViewData["RoutingRuleId"] = new SelectList(_context.RoutingRules, "Id", "Address", destinationMap.RoutingRuleId);
        return View(destinationMap);
    }

    // POST: DestinationMaps/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProtocolConnectionId,RoutingRuleId,Address,Id,CreatedAt,UpdatedAt")] DestinationMap destinationMap)
    {
        if (id != destinationMap.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(destinationMap);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DestinationMapExists(destinationMap.Id))
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
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", destinationMap.ProtocolConnectionId);
        ViewData["RoutingRuleId"] = new SelectList(_context.RoutingRules, "Id", "Address", destinationMap.RoutingRuleId);
        return View(destinationMap);
    }

    // GET: DestinationMaps/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var destinationMap = await _context.DestinationMaps
            .Include(d => d.ProtocolConnection)
            .Include(d => d.RoutingRule)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (destinationMap == null)
        {
            return NotFound();
        }

        return View(destinationMap);
    }

    // POST: DestinationMaps/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var destinationMap = await _context.DestinationMaps.FindAsync(id);
        if (destinationMap != null)
        {
            _context.DestinationMaps.Remove(destinationMap);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetProtocolConnections()
    {
        var protocolConnections = await _context.ProtocolConnections
            .Select(pc => new { value = pc.Id, text = pc.Name })
            .ToListAsync();
        return Json(protocolConnections);
    }

    private bool DestinationMapExists(int id)
    {
        return _context.DestinationMaps.Any(e => e.Id == id);
    }
}
