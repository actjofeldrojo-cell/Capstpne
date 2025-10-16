using CAPS.Migrations;
using CAPS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        public ActionResult UpSert(int? id = null, int? clientId = null)
        {
            if (id == null)
            {
                var transaction = new Transaction 
                { 
                    TransactionDate = DateTime.Now,
                    DateCreated = DateTime.Now,
                    IsActive = true,
                    Status = "Completed", 
                    PaymentMethod = "Cash"
                };

                if (clientId.HasValue)
                {
                    var client = db.Clients
                        .Include(c => c.Appointments)
                            .ThenInclude(a => a.Service)
                        .FirstOrDefault(c => c.ClientId == clientId.Value && c.IsActive);
                    
                    if (client != null)
                    {
                        transaction.AppointmentId = client.Appointments.OrderByDescending(q => q.DateCreated).ToList()[0].AppointmentId;
                        transaction.ClientId = client.ClientId;
                        
                        // Check client's transaction count
                        var transactionCount = db.Transactions
                            .Where(t => t.ClientId == client.ClientId && t.IsActive)
                            .Count();
                        
                        ViewBag.ClientTransactionCount = transactionCount;
                        ViewBag.IsEligibleForDiscount = transactionCount >= 5;
                        
                        var availedServices = client.Appointments
                            .Where(a => a.IsActive && a.Duration > 0)
                            .GroupBy(a => a.ServiceId)
                            .Select(g => new AvailedServiceViewModel { 
                                Service = g.First().Service, 
                                TotalDuration = g.Sum(a => a.Duration) 
                            })
                            .ToList();
                        
                        ViewBag.AvailedServices = availedServices;
                        
                        // Calculate total amount for all services
                        decimal totalAmount = availedServices.Sum(s => s.Service.Price);
                        transaction.Amount = totalAmount;
                        transaction.TotalAmount = totalAmount;
                        
                        transaction.PaymentMethod = "Cash";
                        transaction.Status = "Pending";
                    }
                }

                ViewBag.Clients = db.Clients.Where(c => c.IsActive).ToList();
                ViewBag.Services = db.Services.Where(s => s.isActive).ToList();
                ViewBag.Staff = db.Staffs.Where(s => s.IsActive).ToList();
                ViewBag.Products = db.Products.Where(p => p.IsActive).ToList();

                return View(transaction);
            }
            else
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

                ViewBag.Clients = db.Clients.Where(c => c.IsActive).ToList();
                ViewBag.Services = db.Services.Where(s => s.isActive).ToList();
                ViewBag.Staff = db.Staffs.Where(s => s.IsActive).ToList();

                ViewBag.Products = db.Products.Where(p => p.IsActive).ToList();

                // Load products used for this transaction if editing
                ViewBag.ProductsUsed = db.ProductUsed
                    .Include(pu => pu.Product)
                    .Where(pu => pu.ServiceId == transaction.ServiceId && pu.IsActive)
                    .ToList();

                return View(transaction);
            }
        }

        // POST: Transaction/UpSert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpSert(Transaction transaction, decimal? tenderedAmount = null, decimal? discountPercentage = null, List<ProductUsedViewModel>? productUsed = null)
        {
            ModelState.Remove("Staff");
            ModelState.Remove("Client");
            ModelState.Remove("Service");
            ModelState.Remove("Appointment");

            if (ModelState.IsValid)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (transaction.TransactionId == 0)
                        {
                            var client = db.Clients
                                .Include(c => c.Appointments)
                                    .ThenInclude(a => a.Service)
                                .FirstOrDefault(c => c.ClientId == transaction.ClientId && c.IsActive);
                            
                            if (client != null)
                            {
                                var availedServices = client.Appointments
                                    .Where(a => a.IsActive && a.Duration > 0)
                                    .GroupBy(a => a.ServiceId)
                                    .Select(g => new { 
                                        Service = g.First().Service, 
                                        TotalDuration = g.Sum(a => a.Duration),
                                        TotalCost = g.Sum(a => a.Cost) ?? 0m
                                    })
                                    .ToList();

                                if (availedServices.Any())
                                {
                                    string receiptNumber = GenerateReceiptNumber();
                                    
                                    decimal totalAmount = availedServices.Sum(s => s.TotalCost);
                                    
                                    // Apply discount if provided
                                    decimal finalAmount = totalAmount;
                                    if (discountPercentage.HasValue && discountPercentage.Value > 0)
                                    {
                                        decimal discount = totalAmount * (discountPercentage.Value / 100);
                                        finalAmount = totalAmount - discount;
                                        if (finalAmount < 0) finalAmount = 0;
                                    }
                                    
                                    foreach (var availedService in availedServices)
                                    {
                                        // Calculate proportional discount for this service
                                        decimal serviceAmount = availedService.TotalCost;
                                        decimal serviceDiscount = 0;
                                        
                                        if (discountPercentage.HasValue && discountPercentage.Value > 0)
                                        {
                                            serviceDiscount = availedService.TotalCost * (discountPercentage.Value / 100);
                                        }
                                        
                                        decimal finalServiceAmount = serviceAmount - serviceDiscount;
                                        if (finalServiceAmount < 0) finalServiceAmount = 0;
                                        
                                        var serviceTransaction = new Transaction
                                        {
                                            ClientId = transaction.ClientId,
                                            ServiceId = availedService.Service.ServiceId,
                                            StaffId = transaction.StaffId,
                                            TransactionDate = DateTime.Now,
                                            Amount = serviceAmount,
                                            PaymentMethod = transaction.PaymentMethod,
                                            Status = "Completed",
                                            Notes = transaction.Notes,
                                            DateCreated = DateTime.Now,
                                            IsActive = true,
                                            ReceiptNumber = receiptNumber,
                                            TotalAmount = finalServiceAmount,
                                            DiscountAmount = serviceDiscount,
                                            DiscountPercentage = discountPercentage ?? 0,
                                            AppointmentId = transaction.AppointmentId
                                        };

                                        Models.Appointment appointment = db.Appointments.Find(transaction.AppointmentId);
                                        appointment.Status = "Completed";
                                        db.Appointments.Update(appointment);

                                        db.Transactions.Add(serviceTransaction);
                                        db.SaveChanges(); // Save to get the transaction ID
                                        
                                        // Add products used for this transaction
                                        if (productUsed != null && productUsed.Any())
                                        {
                                            foreach (var product in productUsed.Where(p => p.ProductId > 0 && p.Quantity > 0))
                                            {
                                                var productUsedEntity = new ProductUsed
                                                {
                                                    ProductId = product.ProductId,
                                                    ServiceId = availedService.Service.ServiceId,
                                                    TransactionId = serviceTransaction.TransactionId,
                                                    Quantity = product.Quantity,
                                                    DateUsed = DateTime.Now,
                                                    DateCreated = DateTime.Now,
                                                    IsActive = true
                                                };

                                                Products mainProduct = db.Products.Find(product.ProductId);
                                                mainProduct.StockQuantity -= productUsedEntity.Quantity;

                                                db.Products.Update(mainProduct);
                                                db.ProductUsed.Add(productUsedEntity);
                                            }
                                        }
                                    }
                                    
                                    decimal changeAmount = 0;
                                    if (tenderedAmount.HasValue && tenderedAmount.Value > 0)
                                    {
                                        changeAmount = tenderedAmount.Value - finalAmount;
                                        if (changeAmount < 0)
                                        {
                                            ModelState.AddModelError("", $"Insufficient payment. Amount due: {finalAmount:C}, Tendered: {tenderedAmount:C}");
                                            dbTransaction.Rollback();
                                            ViewBag.Clients = db.Clients.Where(c => c.IsActive).ToList();
                                            ViewBag.Services = db.Services.Where(s => s.isActive).ToList();
                                            ViewBag.Staff = db.Staffs.Where(s => s.IsActive).ToList();
                                            return View(transaction);
                                        }
                                        
                                        var firstTransaction = db.Transactions
                                            .Where(t => t.ReceiptNumber == receiptNumber)
                                            .OrderBy(t => t.TransactionId)
                                            .FirstOrDefault();
                                        
                                        if (firstTransaction != null)
                                        {
                                            firstTransaction.Notes = $"{firstTransaction.Notes}\nPayment: {tenderedAmount:C}, Change: {changeAmount:C}";
                                        }
                                    }

                                    db.SaveChanges();
                                    dbTransaction.Commit();
                                    
                                    TempData["SuccessMessage"] = $"Payment completed successfully! {availedServices.Count} service(s) processed. Receipt: {receiptNumber}";
                                }
                                else
                                {
                                    ModelState.AddModelError("", "No availed services found for this client.");
                                    dbTransaction.Rollback();
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Client not found.");
                                dbTransaction.Rollback();
                            }
                        }
                        else
                        {
                            transaction.DateModified = DateTime.Now;
                            db.Transactions.Update(transaction);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            
                            TempData["SuccessMessage"] = "Transaction updated successfully!";
                        }
                        
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        ModelState.AddModelError("", $"Error processing payment: {ex.Message}");
                    }
                }
            }

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
            var transaction = db.Transactions.Find(id);
            if (transaction != null)
            {
                transaction.IsActive = false;
                transaction.DateModified = DateTime.Now;
                db.Transactions.Update(transaction);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Transaction deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Transaction not found.";
            }
            
            return RedirectToAction(nameof(Index));
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
            var service = db.Services.FirstOrDefault(s => s.ServiceId == serviceId);
            if (service != null)
            {
                return Json(new { price = service.Price });
            }
            return Json(new { price = 0 });
        }

        // AJAX method to get service duration
        [HttpGet]
        public ActionResult GetServiceDuration(int serviceId)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceId == serviceId);
            if (service != null)
            {
                return Json(new { duration = service.Duration });
            }
            return Json(new { duration = 0 });
        }

        // AJAX method to get product information
        [HttpGet]
        public ActionResult GetProductInfo(int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                return Json(new { 
                    name = product.Name, 
                    category = product.Category,
                    stockQuantity = product.StockQuantity,
                    isActive = product.IsActive
                });
            }
            return Json(new { name = "", category = "", stockQuantity = 0, isActive = false });
        }

        // GET: Transaction/Report
        public ActionResult Report(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Service)
                .Include(t => t.Staff)
                .Where(t => t.IsActive);

            // Apply date filters if provided
            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate.Date <= toDate.Value.Date);
            }

            var transactions = query.OrderByDescending(t => t.TransactionDate).ToList();

            // Calculate summary statistics
            var totalTransactions = transactions.Count;
            var totalRevenue = transactions.Sum(t => t.TotalAmount);
            var averageValue = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;
            var todayTransactions = transactions.Count(t => t.TransactionDate.Date == DateTime.Today);

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalTransactions = totalTransactions;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.AverageValue = averageValue;
            ViewBag.TodayTransactions = todayTransactions;

            return View(transactions);
        }
    }

    // ViewModel for availed services
    public class AvailedServiceViewModel
    {
        public Service Service { get; set; }
        public int TotalDuration { get; set; }
    }

    // ViewModel for products used
    public class ProductUsedViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}