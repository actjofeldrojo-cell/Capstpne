using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAPS.Models;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CAPS.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Appointment
        public async Task<IActionResult> Index(int? staffId)
        {
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.IsActive);

            if (staffId.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.StaffId == staffId.Value);
                var staff = await _context.Staffs.FindAsync(staffId.Value);
                ViewBag.FilteredStaff = staff?.FullName;
            }

            var appointments = await appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
                
            // Get active products for the completion modal
            ViewBag.Products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
                
            return View(appointments);
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointment/UpSert/5 (for combined Create/Edit view)
        public async Task<IActionResult> UpSert(int? id)
        {
            ViewBag.Services = await _context.Services.Where(s => s.isActive).ToListAsync();
            ViewBag.Staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
            
            if (id == null || id == 0)
            {
                // Create new appointment
                return View(new AppointmentWithClientDto());
            }
            else
            {
                // Edit existing appointment
                var appointment = await _context.Appointments
                    .Include(a => a.Client)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);
                    
                if (appointment == null)
                {
                    return NotFound();
                }
                
                var dto = new AppointmentWithClientDto
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = appointment.ServiceId,
                    StaffId = appointment.StaffId,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Duration = appointment.Duration,
                    Cost = appointment.Cost,
                    Notes = appointment.Notes,
                    ClientFirstName = appointment.Client?.FirstName,
                    ClientLastName = appointment.Client?.LastName,
                    ClientPhoneNumber = appointment.Client?.PhoneNumber,
                    ClientGender = appointment.Client?.Gender,
                    ClientAge = appointment.Client?.Age
                };
                
                return View(dto);
            }
        }

        // POST: Appointment/UpSert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(AppointmentWithClientDto appointmentDto)
        {
            // Remove validation for navigation properties that aren't being set
            ModelState.Remove("Client");
            ModelState.Remove("Service");
            ModelState.Remove("Staff");

            if (ModelState.IsValid)
            {
                Client client;
                
                if (appointmentDto.AppointmentId == 0)
                {
                    // Check if client exists by phone number
                    client = await _context.Clients
                        .FirstOrDefaultAsync(c => c.PhoneNumber == appointmentDto.ClientPhoneNumber && c.IsActive);
                    
                    if (client == null)
                    {
                        // Create new client
                        client = new Client
                        {
                            FirstName = appointmentDto.ClientFirstName,
                            LastName = appointmentDto.ClientLastName,
                            PhoneNumber = appointmentDto.ClientPhoneNumber,
                            Gender = appointmentDto.ClientGender,
                            Age = appointmentDto.ClientAge,
                            IsActive = true,
                            DateRegistered = DateTime.Now
                        };
                        _context.Clients.Add(client);
                        await _context.SaveChangesAsync();
                    }
                    
                    // Create new appointment
                    var appointment = new Appointment
                    {
                        ClientId = client.ClientId,
                        ServiceId = appointmentDto.ServiceId,
                        StaffId = appointmentDto.StaffId,
                        AppointmentDate = appointmentDto.AppointmentDate,
                        AppointmentTime = appointmentDto.AppointmentTime,
                        Duration = appointmentDto.Duration,
                        Cost = appointmentDto.Cost,
                        Notes = appointmentDto.Notes,
                        DateCreated = DateTime.Now,
                        IsActive = true,
                        Status = "Scheduled"
                    };
                    
                    _context.Add(appointment);
                    TempData["SuccessMessage"] = "Appointment created successfully!";
                }
                else
                {
                    // Update existing appointment
                    var appointment = await _context.Appointments.FindAsync(appointmentDto.AppointmentId);
                    if (appointment != null)
                    {
                        appointment.ServiceId = appointmentDto.ServiceId;
                        appointment.StaffId = appointmentDto.StaffId;
                        appointment.AppointmentDate = appointmentDto.AppointmentDate;
                        appointment.AppointmentTime = appointmentDto.AppointmentTime;
                        appointment.Duration = appointmentDto.Duration;
                        appointment.Cost = appointmentDto.Cost;
                        appointment.Notes = appointmentDto.Notes;
                        appointment.DateModified = DateTime.Now;
                        
                        _context.Update(appointment);
                        TempData["SuccessMessage"] = "Appointment updated successfully!";
                    }
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.isActive).ToListAsync();
            ViewBag.Staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
            return View(appointmentDto);
        }

        // GET: Appointment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                // Soft delete - just mark as inactive
                appointment.IsActive = false;
                appointment.DateModified = DateTime.Now;
                appointment.Status = "Cancelled";
                appointment.CancellationDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointment/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointment/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string cancellationReason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.CancellationReason = cancellationReason;
                appointment.CancellationDate = DateTime.Now;
                appointment.CancelledBy = "System"; // You can replace with actual user
                appointment.DateModified = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointment/Complete/5 (for debugging)
        [HttpGet]
        public IActionResult Complete(int id)
        {
            return RedirectToAction(nameof(Index));
        }

        // POST: Appointment/Complete/5
        [HttpPost, ActionName("Complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteConfirmed(int id, [FromForm] List<int> ProductIds, [FromForm] List<int> Quantities, [FromForm] string CompletionNotes)
        {
            // Debug logging
            Console.WriteLine($"CompleteConfirmed called with id: {id}");
            Console.WriteLine($"ProductIds: {string.Join(", ", ProductIds ?? new List<int>())}");
            Console.WriteLine($"Quantities: {string.Join(", ", Quantities ?? new List<int>())}");
            Console.WriteLine($"CompletionNotes: {CompletionNotes}");
            
            // Try to get form data manually if the model binding fails
            if (ProductIds == null || Quantities == null)
            {
                var form = Request.Form;
                Console.WriteLine($"Form keys: {string.Join(", ", form.Keys)}");
                if (form.ContainsKey("ProductIds[]"))
                {
                    ProductIds = form["ProductIds[]"].Select(int.Parse).ToList();
                }
                else if (form.ContainsKey("ProductIds"))
                {
                    ProductIds = form["ProductIds"].Select(int.Parse).ToList();
                }
                if (form.ContainsKey("Quantities[]"))
                {
                    Quantities = form["Quantities[]"].Select(int.Parse).ToList();
                }
                else if (form.ContainsKey("Quantities"))
                {
                    Quantities = form["Quantities"].Select(int.Parse).ToList();
                }
                if (form.ContainsKey("CompletionNotes"))
                {
                    CompletionNotes = form["CompletionNotes"].FirstOrDefault();
                }
            }
            
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
            
            try
            {
                appointment.Status = "Completed";
                
                // Build products used information
                var productsUsedInfo = "";
                if (ProductIds != null && Quantities != null)
                {
                    var productsList = new List<string>();
                    for (int i = 0; i < ProductIds.Count; i++)
                    {
                        if (ProductIds[i] > 0 && Quantities[i] > 0)
                        {
                            var product = await _context.Products.FindAsync(ProductIds[i]);
                            if (product != null)
                            {
                                productsList.Add($"{Quantities[i]}x {product.Name}");
                            }
                        }
                    }
                    if (productsList.Any())
                    {
                        productsUsedInfo = $"\n\n[PRODUCTS USED] {string.Join(", ", productsList)}";
                    }
                }
                
                // Append completion notes and products used to existing notes
                var completionInfo = "";
                if (!string.IsNullOrEmpty(CompletionNotes))
                {
                    var timestamp = DateTime.Now.ToString("MMM dd, yyyy HH:mm");
                    completionInfo = $"\n\n[COMPLETED {timestamp}] {CompletionNotes}";
                }
                
                appointment.Notes = (appointment.Notes ?? "") + productsUsedInfo + completionInfo;
                appointment.DateModified = DateTime.Now;
                await _context.SaveChangesAsync();
                
                var totalProducts = ProductIds?.Count(p => p > 0) ?? 0;
                TempData["SuccessMessage"] = $"Appointment marked as completed! Products used: {totalProducts}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing appointment: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while completing the appointment.";
                return RedirectToAction(nameof(Index));
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointment/Calendar
        public async Task<IActionResult> Calendar()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.IsActive && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
            return View(appointments);
        }

        // GET: Appointment/Today
        public async Task<IActionResult> Today()
        {
            var today = DateTime.Today;
            var appointments = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .Where(a => a.IsActive && a.AppointmentDate == today)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
            return View(appointments);
        }

        // POST: Appointment/AssignStaff
        [HttpPost]
        public async Task<IActionResult> AssignStaff([FromBody] AssignStaffRequest request)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found." });
                }

                var staff = await _context.Staffs.FindAsync(request.StaffId);
                if (staff == null)
                {
                    return Json(new { success = false, message = "Staff member not found." });
                }

                appointment.StaffId = request.StaffId;
                appointment.DateModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Staff assigned successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while assigning staff." });
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }
    }

    public class AssignStaffRequest
    {
        public int AppointmentId { get; set; }
        public int StaffId { get; set; }
    }

    public class AppointmentWithClientDto
    {
        public int AppointmentId { get; set; }
        
        [Required]
        public int ServiceId { get; set; }
        public int? StaffId { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        
        [Required]
        public int Duration { get; set; }
        public decimal? Cost { get; set; }
        public string? Notes { get; set; }
        
        // Client Information
        [Required]
        [StringLength(100)]
        public string? ClientFirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? ClientLastName { get; set; }
        
        [Required]
        [Phone]
        [StringLength(20)]
        public string? ClientPhoneNumber { get; set; }
        
        [Required]
        [StringLength(20)]
        public string? ClientGender { get; set; }
        
        [StringLength(100)]
        public string? ClientAge { get; set; }
    }
}
