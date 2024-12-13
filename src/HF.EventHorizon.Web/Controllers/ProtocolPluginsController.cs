using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HF.EventHorizon.Core.Entities;
using HF.EventHorizon.Infrastructure.Data;

namespace HF.EventHorizon.Web.Controllers
{
    public class ProtocolPluginsController : Controller
    {
        private readonly EvtHorizonContext _context;

        public ProtocolPluginsController(EvtHorizonContext context)
        {
            _context = context;
        }

        // GET: ProtocolPlugins
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProtocolPlugins.ToListAsync());
        }

        // GET: ProtocolPlugins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var protocolPlugin = await _context.ProtocolPlugins
                .FirstOrDefaultAsync(m => m.Id == id);
            if (protocolPlugin == null)
            {
                return NotFound();
            }

            return View(protocolPlugin);
        }

        // GET: ProtocolPlugins/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProtocolPlugins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Version,PluginDirectoryPath,PluginTypesCsv,RequiredParametersCsv,Id,CreatedAt,UpdatedAt")] ProtocolPlugin protocolPlugin)
        {
            if (ModelState.IsValid)
            {
                _context.Add(protocolPlugin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(protocolPlugin);
        }

        // GET: ProtocolPlugins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var protocolPlugin = await _context.ProtocolPlugins.FindAsync(id);
            if (protocolPlugin == null)
            {
                return NotFound();
            }
            return View(protocolPlugin);
        }

        // POST: ProtocolPlugins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Version,PluginDirectoryPath,PluginTypesCsv,RequiredParametersCsv,Id,CreatedAt,UpdatedAt")] ProtocolPlugin protocolPlugin)
        {
            if (id != protocolPlugin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(protocolPlugin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProtocolPluginExists(protocolPlugin.Id))
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
            return View(protocolPlugin);
        }

        // GET: ProtocolPlugins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var protocolPlugin = await _context.ProtocolPlugins
                .FirstOrDefaultAsync(m => m.Id == id);
            if (protocolPlugin == null)
            {
                return NotFound();
            }

            return View(protocolPlugin);
        }

        // POST: ProtocolPlugins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var protocolPlugin = await _context.ProtocolPlugins.FindAsync(id);
            if (protocolPlugin != null)
            {
                _context.ProtocolPlugins.Remove(protocolPlugin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProtocolPluginExists(int id)
        {
            return _context.ProtocolPlugins.Any(e => e.Id == id);
        }
    }
}
