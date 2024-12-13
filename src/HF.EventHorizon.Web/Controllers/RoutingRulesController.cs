using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using HF.EventHorizon.Core.Entities;
using HF.EventHorizon.Infrastructure.Data;

namespace HF.EventHorizon.Web.Controllers;

public class RoutingRulesController : Controller
{
    private readonly EvtHorizonContext _context;

    public RoutingRulesController(EvtHorizonContext context)
    {
        _context = context;
    }

    // GET: RoutingRules
    public async Task<IActionResult> Index()
    {
        var evtHorizonContext = _context.RoutingRules.Include(r => r.ProtocolConnection);
        return View(await evtHorizonContext.ToListAsync());
    }

    // GET: RoutingRules/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var routingRule = await _context.RoutingRules
            .Include(r => r.ProtocolConnection)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (routingRule == null)
        {
            return NotFound();
        }

        return View(routingRule);
    }

    // GET: RoutingRules/Create
    public IActionResult Create()
    {
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson");
        return View();
    }

    // POST: RoutingRules/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProtocolConnectionId,Address,Id,CreatedAt,UpdatedAt")] RoutingRule routingRule)
    {
        if (ModelState.IsValid)
        {
            _context.Add(routingRule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", routingRule.ProtocolConnectionId);
        return View(routingRule);
    }

    // GET: RoutingRules/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var routingRule = await _context.RoutingRules.FindAsync(id);
        if (routingRule == null)
        {
            return NotFound();
        }
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", routingRule.ProtocolConnectionId);
        return View(routingRule);
    }

    // POST: RoutingRules/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProtocolConnectionId,Address,Id,CreatedAt,UpdatedAt")] RoutingRule routingRule)
    {
        if (id != routingRule.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(routingRule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoutingRuleExists(routingRule.Id))
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
        ViewData["ProtocolConnectionId"] = new SelectList(_context.ProtocolConnections, "Id", "AdditionalParametersJson", routingRule.ProtocolConnectionId);
        return View(routingRule);
    }

    // GET: RoutingRules/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var routingRule = await _context.RoutingRules
            .Include(r => r.ProtocolConnection)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (routingRule == null)
        {
            return NotFound();
        }

        return View(routingRule);
    }

    // POST: RoutingRules/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var routingRule = await _context.RoutingRules.FindAsync(id);
        if (routingRule != null)
        {
            _context.RoutingRules.Remove(routingRule);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RoutingRuleExists(int id)
    {
        return _context.RoutingRules.Any(e => e.Id == id);
    }
}
