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
        public async Task<IActionResult> Index()
        {
            // First, automatically delete appointments older than 1 day
            await DeleteOldAppointments();

            var appointmentsQuery = _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.IsActive);

            var appointments = await appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();
                
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
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointment/UpSert/5 (for combined Create/Edit view)
        public async Task<IActionResult> UpSert(int? id, string? date = null, string? time = null)
        {
            ViewBag.Services = await _context.Services.Where(s => s.isActive).ToListAsync();
            
            if (id == null || id == 0)
            {
                // Create new appointment
                var dto = new AppointmentWithClientDto();
                if (DateTime.TryParse(date, out var parsedDate))
                {
                    dto.AppointmentDate = parsedDate;
                }
                if (TimeSpan.TryParse(time, out var parsedTime))
                {
                    dto.AppointmentTime = parsedTime;
                }
                return View(dto);
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
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Duration = appointment.Duration,
                    Cost = appointment.Cost,
                    Notes = appointment.Notes,
                    ClientFirstName = appointment.Client?.FirstName,
                    ClientLastName = appointment.Client?.LastName,
                    ClientPhoneNumber = appointment.Client?.PhoneNumber,
                    PreferredTherapistGender = appointment.Client?.PreferredTherapistGender,
                    MassagePressureLevel = appointment.Client?.MassagePressureLevel,
                    MusicPreference = appointment.Client?.MusicPreference,
                    TemperaturePreference = appointment.Client?.TemperaturePreference,
                    ComfortItemPreferences = appointment.Client?.ComfortItemPreferences
                };
                
                return View(dto);
            }
        }

        // POST: Appointment/UpSert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(AppointmentWithClientDto appointmentDto, string[] ComfortItemPreferences)
        {
            // Handle checkbox values for comfort items
            if (ComfortItemPreferences != null && ComfortItemPreferences.Length > 0)
            {
                appointmentDto.ComfortItemPreferences = string.Join(", ", ComfortItemPreferences);
            }

            // Remove validation for navigation properties that aren't being set
            ModelState.Remove("Client");
            ModelState.Remove("Service");

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
                            PreferredTherapistGender = appointmentDto.PreferredTherapistGender,
                            MassagePressureLevel = appointmentDto.MassagePressureLevel,
                            MusicPreference = appointmentDto.MusicPreference,
                            TemperaturePreference = appointmentDto.TemperaturePreference,
                            ComfortItemPreferences = appointmentDto.ComfortItemPreferences,
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

        // GET: Appointment/UserAppointments
        public async Task<IActionResult> UserAppointments()
        {
            // For now, we'll use a simple approach - in a real app you'd get the user ID from authentication
            // This is a placeholder for user-specific appointment viewing
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime);

            var appointments = await appointmentsQuery.ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.isActive).ToListAsync();
            
            return View(appointments);
        }

        // GET: Appointment/Book
        public async Task<IActionResult> Book()
        {
            return View(new AppointmentWithClientDto());
        }

        // POST: Appointment/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentWithClientDto appointmentDto, string[] ComfortItemPreferences)
        {
            // Handle checkbox values for comfort items
            if (ComfortItemPreferences != null && ComfortItemPreferences.Length > 0)
            {
                appointmentDto.ComfortItemPreferences = string.Join(", ", ComfortItemPreferences);
            }

            // Remove validation for navigation properties that aren't being set
            ModelState.Remove("Client");
            ModelState.Remove("Service");
            ModelState.Remove("ServiceId");
            ModelState.Remove("AppointmentDate");
            ModelState.Remove("AppointmentTime");
            ModelState.Remove("Duration");
            ModelState.Remove("Cost");
            ModelState.Remove("Notes");

            if (ModelState.IsValid)
            {
                // Check if client exists by phone number
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.PhoneNumber == appointmentDto.ClientPhoneNumber && c.IsActive);
                
                if (existingClient == null)
                {
                    // Create new client
                    var client = new Client
                    {
                        FirstName = appointmentDto.ClientFirstName,
                        LastName = appointmentDto.ClientLastName,
                        PhoneNumber = appointmentDto.ClientPhoneNumber,
                        PreferredTherapistGender = appointmentDto.PreferredTherapistGender,
                        MassagePressureLevel = appointmentDto.MassagePressureLevel,
                        MusicPreference = appointmentDto.MusicPreference,
                        TemperaturePreference = appointmentDto.TemperaturePreference,
                        ComfortItemPreferences = appointmentDto.ComfortItemPreferences,
                        IsActive = true,
                        DateRegistered = DateTime.Now
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Client registered successfully!";
                }
                else
                {
                    TempData["InfoMessage"] = "Client with this phone number already exists.";
                }
                
                return RedirectToAction("Index", "Home");
            }
            
            return View(appointmentDto);
        }

        // GET: Appointment/UserEdit/5
        public async Task<IActionResult> UserEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.Services = await _context.Services.Where(s => s.isActive).ToListAsync();
            
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
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Duration = appointment.Duration,
                Cost = appointment.Cost,
                Notes = appointment.Notes,
                ClientFirstName = appointment.Client?.FirstName,
                ClientLastName = appointment.Client?.LastName,
                ClientPhoneNumber = appointment.Client?.PhoneNumber,
                PreferredTherapistGender = appointment.Client?.PreferredTherapistGender,
                MassagePressureLevel = appointment.Client?.MassagePressureLevel,
                MusicPreference = appointment.Client?.MusicPreference,
                TemperaturePreference = appointment.Client?.TemperaturePreference,
                ComfortItemPreferences = appointment.Client?.ComfortItemPreferences
            };
            
            return View(dto);
        }

        // POST: Appointment/UserEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(int id, AppointmentWithClientDto appointmentDto)
        {
            if (id != appointmentDto.AppointmentId)
            {
                return NotFound();
            }

            // Remove validation for navigation properties that aren't being set
            ModelState.Remove("Client");
            ModelState.Remove("Service");

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    // Update appointment details
                    appointment.ServiceId = appointmentDto.ServiceId;
                    appointment.AppointmentDate = appointmentDto.AppointmentDate;
                    appointment.AppointmentTime = appointmentDto.AppointmentTime;
                    appointment.Duration = appointmentDto.Duration;
                    appointment.Cost = appointmentDto.Cost;
                    appointment.Notes = appointmentDto.Notes;
                    appointment.DateModified = DateTime.Now;
                    
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Appointment updated successfully!";
                    return RedirectToAction(nameof(UserAppointments));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            ViewBag.Services = await _context.Services.Where(s => s.isActive).ToListAsync();
            return View(appointmentDto);
        }

        // POST: Appointment/UserCancel/5
        [HttpPost, ActionName("UserCancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCancelConfirmed(int id, string cancellationReason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.CancellationReason = cancellationReason ?? "Cancelled by user";
                appointment.CancellationDate = DateTime.Now;
                appointment.CancelledBy = "User";
                appointment.DateModified = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }
            return RedirectToAction(nameof(UserAppointments));
        }

        // POST: Appointment/Confirm/5
        [HttpPost, ActionName("Confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Confirmed";
                appointment.DateModified = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment confirmed successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Appointment/Complete/5
        [HttpPost, ActionName("Complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Completed";
                appointment.DateModified = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment marked as completed!";
            }
            return RedirectToAction(nameof(Index));
        }



        // GET: Appointment/Calendar
        public async Task<IActionResult> Calendar()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.IsActive && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
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
                .Where(a => a.IsActive && a.AppointmentDate == today)
                .OrderByDescending(a => a.AppointmentTime)
                .ToListAsync();
            return View(appointments);
        }



        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        // Helper method to automatically delete appointments older than 1 day
        private async Task DeleteOldAppointments()
        {
            var oneDayAgo = DateTime.Today.AddDays(-1);
            
            var oldAppointments = await _context.Appointments
                .Where(a => a.IsActive && a.AppointmentDate < oneDayAgo)
                .ToListAsync();

            if (oldAppointments.Any())
            {
                foreach (var appointment in oldAppointments)
                {
                    // Soft delete - mark as inactive
                    appointment.IsActive = false;
                    appointment.DateModified = DateTime.Now;
                    appointment.Status = "Auto-Deleted";
                    appointment.CancellationReason = "Automatically deleted - appointment date is more than 1 day old";
                    appointment.CancellationDate = DateTime.Now;
                    appointment.CancelledBy = "System";
                }

                await _context.SaveChangesAsync();
            }
        }
    }



    public class AppointmentWithClientDto
    {
        public int AppointmentId { get; set; }
        
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
        
        // Client Preferences
        [StringLength(50)]
        public string? PreferredTherapistGender { get; set; }

        [StringLength(20)]
        public string? MassagePressureLevel { get; set; }

        [StringLength(50)]
        public string? MusicPreference { get; set; }

        [StringLength(50)]
        public string? TemperaturePreference { get; set; }

        [StringLength(200)]
        public string? ComfortItemPreferences { get; set; }
        public int ServiceId { get; internal set; }
        public DateTime AppointmentDate { get; internal set; }
        public TimeSpan AppointmentTime { get; internal set; }
        public int Duration { get; internal set; }
        public decimal? Cost { get; internal set; }
        public string Notes { get; internal set; }
    }
}
