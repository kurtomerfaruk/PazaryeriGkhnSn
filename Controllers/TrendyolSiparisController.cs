using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class TrendyolSiparisController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TrendyolSiparisService _service;

        public TrendyolSiparisController(AppDbContext context, TrendyolSiparisService siparisService)
        {
            _context = context;
            _service = siparisService;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            return View(await _context.TrendyolSiparisler.ToListAsync());
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(TrendyolSiparis trendyolSiparis)
        {
            if (!ModelState.IsValid) return View(trendyolSiparis);

            _context.TrendyolSiparisler.Add(trendyolSiparis);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var trendyolSiparis = await _context.TrendyolSiparisler.FindAsync(id);
            if (trendyolSiparis == null) return NotFound();
            return View(trendyolSiparis);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TrendyolSiparis trendyolSiparis)
        {
            _context.TrendyolSiparisler.Update(trendyolSiparis);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var trendyolSiparis = await _context.TrendyolSiparisler.FindAsync(id);
            if (trendyolSiparis == null) return NotFound();
            return View(trendyolSiparis);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trendyolSiparis = await _context.TrendyolSiparisler.FindAsync(id);
            _context.TrendyolSiparisler.Remove(trendyolSiparis);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
