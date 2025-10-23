using Microsoft.AspNetCore.Mvc;
using CAPS.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CAPS.Attributes;

namespace CAPS.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Home
        public IActionResult Home()
        {
            // Check if user is admin
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                return RedirectToAction("Dashboard");
            }
            
            return RedirectToAction("Login");
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Simple admin authentication (you can enhance this later)
            if (username == "admin" && password == "admin111")
            {
                // Set admin session
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUsername", username);

                TempData["SuccessMessage"] = "Welcome, Admin!";
                return RedirectToAction("Dashboard");
            }
            
            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        // GET: Admin/Dashboard
        [AdminAuthorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);
                var lastMonthEnd = thisMonth.AddDays(-1);

                // Get transaction data with optimized queries
                var allTransactions = await _context.Transactions
                    .Where(t => t.IsActive && t.Status == "Completed")
                    .Select(t => new { t.TransactionDate, t.TotalAmount })
                    .ToListAsync();

                var monthlyTransactions = allTransactions
                    .Where(t => t.TransactionDate >= thisMonth)
                    .ToList();

                var dailyTransactions = allTransactions
                    .Where(t => t.TransactionDate.Date == today)
                    .ToList();

                var lastMonthTransactions = allTransactions
                    .Where(t => t.TransactionDate >= lastMonth && t.TransactionDate <= lastMonthEnd)
                    .ToList();

                // Calculate revenue metrics
                var totalRevenue = allTransactions.Sum(t => t.TotalAmount);
                var monthlyRevenue = monthlyTransactions.Sum(t => t.TotalAmount);
                var dailyRevenue = dailyTransactions.Sum(t => t.TotalAmount);
                var lastMonthRevenue = lastMonthTransactions.Sum(t => t.TotalAmount);

                // Calculate sales metrics
                var totalSales = allTransactions.Count;
                var monthlySales = monthlyTransactions.Count;
                var dailySales = dailyTransactions.Count;

                // Calculate average transaction value
                var averageTransactionValue = totalSales > 0 ? totalRevenue / totalSales : 0;

                // Calculate revenue growth
                var revenueGrowth = lastMonthRevenue > 0 ? 
                    ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

                // Service popularity analysis - optimized query
                var servicePopularity = await _context.Transactions
                    .Where(t => t.IsActive && t.Status == "Completed")
                    .GroupBy(t => t.ServiceId)
                    .Select(g => new ServicePopularityData
                    {
                        ServiceName = _context.Services
                            .Where(s => s.ServiceId == g.Key)
                            .Select(s => s.Name)
                            .FirstOrDefault(),
                        BookingCount = g.Count(),
                        Revenue = g.Sum(t => t.TotalAmount)
                    })
                    .OrderByDescending(s => s.BookingCount)
                    .Take(7)
                    .ToListAsync();

                // Staff performance: include all active staff and count clients served from appointments this month
                var staffPerformance = await _context.Staffs
                    .Where(s => s.IsActive)
                    .Select(s => new StaffPerformanceData
                    {
                        StaffName = s.FullName,
                        ClientsServed = _context.Appointments
                            .Where(a => a.IsActive && a.Status == "Completed" && a.AppointmentDate >= thisMonth && a.StaffId == s.StaffId)
                            .Select(a => a.ClientId)
                            .Distinct()
                            .Count()
                    })
                    .OrderByDescending(sp => sp.ClientsServed)
                    .Take(10)
                    .ToListAsync();

                // Calculate percentages for service popularity
                var totalBookings = servicePopularity.Sum(s => s.BookingCount);
                foreach (var service in servicePopularity)
                {
                    service.Percentage = totalBookings > 0 ? (decimal)service.BookingCount / totalBookings * 100 : 0;
                }

                // Product inventory monitoring
                var allProducts = await _context.Products
                    .Where(p => p.IsActive)
                    .ToListAsync();

                var lowStockProducts = allProducts
                    .Where(p => p.StockQuantity < 5) // Consider low stock if less than 5
                    .ToList();

                // Revenue chart data (last 6 months) - optimized
                var revenueChartData = new List<RevenueChartData>();
                for (int i = 5; i >= 0; i--)
                {
                    var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    
                    var monthTransactions = allTransactions
                        .Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd)
                        .ToList();

                    revenueChartData.Add(new RevenueChartData
                    {
                        Month = monthStart.ToString("MMM"),
                        Revenue = monthTransactions.Sum(t => t.TotalAmount),
                        Sales = monthTransactions.Count
                    });
                }

                // Get dashboard statistics
                var dashboardStats = new AdminDashboardViewModel
                {
                    // Business Performance Metrics
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    DailyRevenue = dailyRevenue,
                    TotalSales = totalSales,
                    MonthlySales = monthlySales,
                    DailySales = dailySales,
                    AverageTransactionValue = averageTransactionValue,
                    RevenueGrowth = revenueGrowth,
                    
                    // Service Popularity
                    ServicePopularity = servicePopularity,
                    
                    // Staff Performance
                    StaffPerformance = staffPerformance,
                    
                    // Product Inventory
                    TotalProducts = allProducts.Count,
                    LowStockProducts = lowStockProducts.Count,
                    LowStockItems = lowStockProducts,
                    
                    // Revenue Chart Data
                    RevenueChartData = revenueChartData,
                    
                    // Legacy data
                    TotalAppointments = await _context.Appointments.CountAsync(),
                    TotalClients = await _context.Clients.CountAsync(),
                    TotalServices = await _context.Services.CountAsync(),
                    TotalStaff = await _context.Staffs.CountAsync(),
                    TotalTransactions = await _context.Transactions.CountAsync(),
                    
                    RecentAppointments = await _context.Appointments
                        .Where(a => a.AppointmentDate != default(DateTime))
                        .OrderByDescending(a => a.AppointmentDate)
                        .Take(5)
                        .ToListAsync(),
                    
                    RecentTransactions = await _context.Transactions
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(5)
                        .ToListAsync(),
                    
                    RecentClients = await _context.Clients
                        .Where(c => c.IsActive)
                        .OrderByDescending(c => c.DateRegistered)
                        .Take(5)
                        .ToListAsync()
                };

                return View(dashboardStats);
            }
            catch (Exception ex)
            {
                // Log the error (you can add proper logging here)
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        // GET: Admin/TestDatabase
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test database connection
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                
                // Test basic queries
                var clientCount = await _context.Clients.CountAsync();
                var serviceCount = await _context.Services.CountAsync();
                
                return Json(new { 
                    success = true, 
                    message = "Database connection successful",
                    clientCount = clientCount,
                    serviceCount = serviceCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = ex.Message,
                    details = ex.ToString()
                });
            }
        }

        // GET: Admin/Logout
        [AdminAuthorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index","Home");
        }

        // Helper method to check if user is admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
    }

    public class AdminDashboardViewModel
    {
        // Business Performance Metrics
        public decimal TotalRevenue { get; set; } = 0;
        public decimal MonthlyRevenue { get; set; } = 0;
        public decimal DailyRevenue { get; set; } = 0;
        public int TotalSales { get; set; } = 0;
        public int MonthlySales { get; set; } = 0;
        public int DailySales { get; set; } = 0;
        public decimal AverageTransactionValue { get; set; } = 0;
        public decimal RevenueGrowth { get; set; } = 0;
        
        // Service Popularity
        public List<ServicePopularityData> ServicePopularity { get; set; } = new List<ServicePopularityData>();
        
        // Staff Performance
        public List<StaffPerformanceData> StaffPerformance { get; set; } = new List<StaffPerformanceData>();
        
        // Product Inventory
        public int TotalProducts { get; set; } = 0;
        public int LowStockProducts { get; set; } = 0;
        public List<Products> LowStockItems { get; set; } = new List<Products>();
        
        // Revenue Chart Data
        public List<RevenueChartData> RevenueChartData { get; set; } = new List<RevenueChartData>();
        
        // Legacy data (keeping for compatibility)
        public int TotalAppointments { get; set; } = 0;
        public int TotalClients { get; set; } = 0;
        public int TotalServices { get; set; } = 0;
        public int TotalStaff { get; set; } = 0;
        public int TotalTransactions { get; set; } = 0;
        
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<Client> RecentClients { get; set; } = new List<Client>();
    }

    public class ServicePopularityData
    {
        public string ServiceName { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RevenueChartData
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int Sales { get; set; }
    }

    public class StaffPerformanceData
    {
        public string StaffName { get; set; }
        public int ClientsServed { get; set; }
    }
}
