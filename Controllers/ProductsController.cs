using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Services;
using System.ComponentModel;
using System.Globalization;

namespace Pazaryeri.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ProductService _service;

        public ProductsController(AppDbContext context,ProductService productService)
        {
            _context = context;
            _service = productService;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportExcel()
        {
            var products = await _service.GetAll();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Liste");

            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Price";

            int row = 2;
            foreach (var p in products)
            {
                ws.Cell(row, 1).Value = p.Id;
                ws.Cell(row, 2).Value = p.Name;
                ws.Cell(row, 3).Value = p.Price;
                ws.Cell(row, 3).Style.NumberFormat.Format = "₺ #,##0.00";
                row++;
            }

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "urunler.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("Index");


            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet(1);


            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var raw = row.Cell(3).GetValue<string>();
                decimal price = decimal.Parse(raw, new CultureInfo("tr-TR"));
                var product = new Product
                {
                    Name = row.Cell(2).GetValue<string>(),
                    Price = price
                };

                await _service.Add(product);
            }

            return RedirectToAction("Index");
        }
    }
}
