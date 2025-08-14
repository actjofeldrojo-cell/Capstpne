using CAPS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAPS.Controllers
{
    public class TransactionController : Controller
    {
        readonly AppDbContext db;
        public TransactionController(AppDbContext db) { this.db = db; }

        // GET: Transaction
        public ActionResult Index()
        {
            var transactions = db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Service)
                .Include(t => t.Staff)
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
            
            return View(transactions);
        }

        // GET: Transaction/Details/5
        public ActionResult Details(int id)
        {
            var transaction = db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Service)
                .Include(t => t.Staff)
                .FirstOrDefault(t => t.TransactionId == id && t.IsActive);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/UpSert
        public ActionResult UpSert(int? id = null)
        {
            if (id == null)
            {
                // Create new transaction
                return View(new Transaction 
                { 
                    TransactionDate = DateTime.Now,
                    DateCreated = DateTime.Now,
                    IsActive = true,
                    Status = "Completed",
                    PaymentMethod = "Cash"
                });
            }
            else
            {
                // Edit existing transaction
                var transaction = db.Transactions
                    .Include(t => t.Client)
                    .Include(t => t.Service)
                    .Include(t => t.Staff)
                    .FirstOrDefault(t => t.TransactionId == id && t.IsActive);

                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }
        }

        // POST: Transaction/UpSert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpSert(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Check if this is a new transaction (TransactionId == 0) or existing one
                        if (transaction.TransactionId == 0)
                        {
                            // New transaction
                            transaction.DateCreated = DateTime.Now;
                            
                            // Generate receipt number if not provided
                            if (string.IsNullOrEmpty(transaction.ReceiptNumber))
                            {
                                transaction.ReceiptNumber = GenerateReceiptNumber();
                            }

                            db.Transactions.Add(transaction);
                        }
                        else
                        {
                            // Existing transaction
                            transaction.DateModified = DateTime.Now;
                            db.Transactions.Update(transaction);
                        }

                        transaction.CalculateTotal(); // Calculate the total amount
                        db.SaveChanges();
                        
                        dbTransaction.Commit();
                        
                        TempData["SuccessMessage"] = transaction.TransactionId == 0 
                            ? "Transaction created successfully!" 
                            : "Transaction updated successfully!";
                        
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        ModelState.AddModelError("", "Error saving transaction. Please try again.");
                        Console.WriteLine($"Error saving transaction: {ex.Message}");
                    }
                }
            }

            // Reload view data if validation fails
            ViewBag.Clients = db.Clients.Where(c => c.IsActive).ToList();
            ViewBag.Services = db.Services.Where(s => s.isActive).ToList();
            ViewBag.Staff = db.Staffs.Where(s => s.IsActive).ToList();
            
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public ActionResult Delete(int id)
        {
            var transaction = db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Service)
                .Include(t => t.Staff)
                .FirstOrDefault(t => t.TransactionId == id && t.IsActive);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var transaction = db.Transactions.Find(id);
                    if (transaction != null)
                    {
                        transaction.IsActive = false;
                        transaction.DateModified = DateTime.Now;
                        
                        db.Transactions.Update(transaction);
                        db.SaveChanges();
                        
                        dbTransaction.Commit();
                        
                        TempData["SuccessMessage"] = "Transaction deleted successfully!";
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    TempData["ErrorMessage"] = "Error deleting transaction. Please try again.";
                    Console.WriteLine($"Error deleting transaction: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Transaction/Receipt/5
        public ActionResult Receipt(int id)
        {
            var transaction = db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Service)
                .Include(t => t.Staff)
                .FirstOrDefault(t => t.TransactionId == id && t.IsActive);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Report
        public ActionResult Report(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Service)
                .Include(t => t.Staff)
                .Where(t => t.IsActive);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate.Date <= endDate.Value.Date);
            }

            var transactions = query.OrderByDescending(t => t.TransactionDate).ToList();

            var reportSummary = new
            {
                TotalTransactions = transactions.Count,
                TotalRevenue = transactions.Sum(t => t.TotalAmount),
                TotalDiscounts = transactions.Sum(t => t.DiscountAmount),
                TotalTaxes = transactions.Sum(t => t.TaxAmount),
                AverageTransactionValue = transactions.Any() ? transactions.Average(t => t.TotalAmount) : 0,
                TopServices = transactions
                    .GroupBy(t => t.Service.Name)
                    .Select(g => new { ServiceName = g.Key, Count = g.Count(), Revenue = g.Sum(t => t.TotalAmount) })
                    .OrderByDescending(x => x.Revenue)
                    .Take(5)
                    .ToList()
            };

            ViewBag.ReportSummary = reportSummary;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(transactions);
        }

        // Helper method to generate receipt number
        private string GenerateReceiptNumber()
        {
            var today = DateTime.Now;
            var prefix = $"RCP{today:yyyyMMdd}";
            var count = db.Transactions
                .Where(t => t.TransactionDate.Date == today.Date)
                .Count() + 1;
            
            return $"{prefix}-{count:D4}";
        }

        // AJAX method to get service price
        [HttpGet]
        public ActionResult GetServicePrice(int serviceId)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceId == serviceId && s.isActive);
            if (service != null)
            {
                return Json(new { success = true, price = service.Price, duration = service.Duration });
            }
            return Json(new { success = false, message = "Service not found" });
        }

        // AJAX method to calculate total
        [HttpPost]
        public ActionResult CalculateTotal([FromBody] TransactionCalculationRequest request)
        {
            try
            {
                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    DiscountAmount = request.DiscountAmount,
                    DiscountPercentage = request.DiscountPercentage,
                    TaxAmount = request.TaxAmount,
                    TaxPercentage = request.TaxPercentage
                };

                transaction.CalculateTotal();

                return Json(new { 
                    success = true, 
                    totalAmount = transaction.TotalAmount,
                    discountAmount = transaction.DiscountAmount,
                    taxAmount = transaction.TaxAmount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // Helper class for AJAX requests
    public class TransactionCalculationRequest
    {
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}
