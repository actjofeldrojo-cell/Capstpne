using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAPS.Models;
using System.Linq;
using System.Threading.Tasks;
using CAPS.Attributes;

namespace CAPS.Controllers
{
    public class ProductController : GenericController
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Product
        [AdminAuthorize]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
            ViewData["Title"] = "Products Management";
            return View(products);
        }

        // GET: Product/UpSert/5 (for combined Create/Edit view)
        [AdminAuthorize]
        public async Task<IActionResult> UpSert(int? id)
        {
            if (id == null || id == 0)
            {
                // Create new product
                return View(new Products());
            }
            else
            {
                // Edit existing product
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
        }

        // POST: Product/UpSert
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> UpSert([Bind("ProductId,Name,Description,Category,StockQuantity")] Products product)
        {
            if (ModelState.IsValid)
            {
                if (product.ProductId == 0)
                {
                    // Create new product
                    _context.Add(product);
                    TempData["SuccessMessage"] = "Product created successfully!";
                }
                else
                {
                    // Update existing product
                    _context.Update(product);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [AdminAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Hard delete
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
