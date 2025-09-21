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
                // Get dashboard statistics
                var dashboardStats = new AdminDashboardViewModel
                {
                    TotalAppointments = await _context.Appointments.CountAsync(),
                    TotalClients = await _context.Clients.CountAsync(),
                    TotalProducts = await _context.Products.CountAsync(),
                    TotalServices = await _context.Services.CountAsync(),
                    TotalStaff = await _context.Staffs.CountAsync(),
                    TotalTransactions = await _context.Transactions.CountAsync(),
                    
                    RecentAppointments = await _context.Appointments
                        .Include(a => a.Client)
                        .Include(a => a.Service)
                        .Where(a => a.AppointmentDate != default(DateTime))
                        .OrderByDescending(a => a.AppointmentDate)
                        .Take(5)
                        .ToListAsync(),
                    
                    RecentTransactions = await _context.Transactions
                        .Include(t => t.Client)
                        .Include(t => t.Service)
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

        // GET: Admin/Logout
        [AdminAuthorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // Helper method to check if user is admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalAppointments { get; set; }
        public int TotalClients { get; set; }
        public int TotalProducts { get; set; }
        public int TotalServices { get; set; }
        public int TotalStaff { get; set; }
        public int TotalTransactions { get; set; }
        
        public List<Appointment> RecentAppointments { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public List<Client> RecentClients { get; set; }
    }
}
