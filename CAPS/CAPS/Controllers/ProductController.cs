using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAPS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CAPS.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }

        // GET: Product/UpSert/5 (for combined Create/Edit view)
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
        public async Task<IActionResult> UpSert([Bind("ProductId,Name,Description,Price,Category,StockQuantity,SKU,Brand,Size,Color,DiscountPercentage,ImageUrl,AdditionalNotes")] Products product)
        {
            if (ModelState.IsValid)
            {
                if (product.ProductId == 0)
                {
                    // Create new product
                    product.DateCreated = DateTime.Now;
                    product.IsActive = true;
                    _context.Add(product);
                    TempData["SuccessMessage"] = "Product created successfully!";
                }
                else
                {
                    // Update existing product
                    product.DateModified = DateTime.Now;
                    _context.Update(product);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Soft delete - just mark as inactive
                product.IsActive = false;
                product.DateModified = DateTime.Now;
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
