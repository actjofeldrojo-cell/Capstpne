using CAPS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CAPS.Controllers
{
    public class ClientController : Controller
    {
        readonly AppDbContext db;
        public ClientController(AppDbContext db) { this.db = db; }

        public async Task<IActionResult> Index()
        {
            // Get completed client IDs from session
            var completedClientIds = HttpContext.Session.GetString("CompletedClients")?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
            
            // Get active clients (excluding completed ones)
            var activeClients = await db.Clients
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Where(c => c.IsActive && !completedClientIds.Contains(c.ClientId)) // Exclude completed clients
                .ToListAsync();
            
            // Get completed clients
            var completedClients = await db.Clients
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Where(c => completedClientIds.Contains(c.ClientId))
                .ToListAsync();

            var allClients = activeClients.Concat(completedClients).ToList();
            
            return View(allClients);
        }

        // Update and Insert Viewing
        public ActionResult UpSert(int? id)
        {
            return View(id == null ? new Client() { IsActive = true } : db.Clients.FirstOrDefault(c => c.ClientId == id));
        }

        [HttpPost]
        public ActionResult UpSert(Client client)
        {
            if (!ModelState.IsValid) { return View(client); }
            if (client.ClientId == 0)
            {
                // Create Client from bound data
                client.IsActive = true;
                db.Clients.Add(client);
            }
            else
            {
                db.Clients.Update(client);
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Client");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                Client client = db.Clients.FirstOrDefault(c => c.ClientId == id);
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Client not found.";
                    return RedirectToAction("Index", "Client");
                }

                // Add client ID to completed clients in session
                var completedClientIds = HttpContext.Session.GetString("CompletedClients")?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
                if (!completedClientIds.Contains(id))
                {
                    completedClientIds.Add(id);
                    HttpContext.Session.SetString("CompletedClients", string.Join(",", completedClientIds));
                }
                
                TempData["SuccessMessage"] = $"Client {client.FirstName} {client.LastName} has been completed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing client: {ex.Message}";
            }

            return RedirectToAction("Index", "Client");
        }

        [HttpPost]
        public ActionResult Cancel(int id)
        {
            try
            {
                Client client = db.Clients.FirstOrDefault(c => c.ClientId == id);
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Client not found.";
                    return RedirectToAction("Index", "Client");
                }

                client.IsActive = false;
                db.Clients.Update(client);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = $"Client {client.FirstName} {client.LastName} has been cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling client: {ex.Message}";
            }

            return RedirectToAction("Index", "Client");
        }

        // Service Extend - Display all services with editable duration
        public async Task<IActionResult> ServiceExtend(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await db.Clients.FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
            {
                return NotFound();
            }

            // Get all active services
            var allServices = await db.Services.Where(s => s.isActive).ToListAsync();

            // Get client's current service durations (if any exist in appointments)
            var clientAppointments = await db.Appointments
                .Include(a => a.Service)
                .Where(a => a.ClientId == id && a.IsActive)
                .ToListAsync();

            // Create a dictionary to track client's current service durations
            var clientServiceDurations = clientAppointments
                .GroupBy(a => a.ServiceId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Duration));

            ViewBag.AllServices = allServices;
            ViewBag.ClientServiceDurations = clientServiceDurations;

            return View(client);
        }

        // POST: ServiceExtend - Update service durations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceExtend(int clientId, Dictionary<int, int> serviceDurations)
        {
            if (serviceDurations == null || !serviceDurations.Any())
            {
                TempData["ErrorMessage"] = "No service durations provided.";
                return RedirectToAction("ServiceExtend", new { id = clientId });
            }

            try
            {
                var client = await db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Client not found.";
                    return RedirectToAction("ServiceExtend", new { id = clientId });
                }

                // Get existing appointments for this client
                var existingAppointments = await db.Appointments
                    .Where(a => a.ClientId == clientId && a.IsActive)
                    .ToListAsync();

                // Update or create appointments for each service
                foreach (var serviceDuration in serviceDurations)
                {
                    var serviceId = serviceDuration.Key;
                    var duration = serviceDuration.Value;

                    // Check if appointment already exists for this service
                    var existingAppointment = existingAppointments
                        .FirstOrDefault(a => a.ServiceId == serviceId);

                    if (existingAppointment != null)
                    {
                        if (duration <= 0)
                        {
                            // Remove appointment if duration is 0 or negative
                            existingAppointment.IsActive = false;
                            existingAppointment.DateModified = DateTime.Now;
                            db.Update(existingAppointment);
                        }
                        else
                        {
                            // Update existing appointment duration
                            // Ensure duration meets validation requirements (15-480 minutes)
                            var validDuration = Math.Max(15, Math.Min(480, duration));
                            existingAppointment.Duration = validDuration;
                            existingAppointment.DateModified = DateTime.Now;
                            existingAppointment.Notes = $"Service duration updated via Service Extend - Original: {duration} min";
                            db.Update(existingAppointment);
                        }
                    }
                    else if (duration > 0)
                    {
                        // Create new appointment for this service only if duration > 0
                        var service = await db.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
                        if (service != null)
                        {
                            // Ensure duration meets validation requirements (15-480 minutes)
                            var validDuration = Math.Max(15, Math.Min(480, duration));
                            
                            var newAppointment = new Appointment
                            {
                                ClientId = clientId,
                                ServiceId = serviceId,
                                Duration = validDuration,
                                Cost = service.Price,
                                AppointmentDate = DateTime.Today,
                                AppointmentTime = TimeSpan.FromHours(9), // Default time
                                Status = "Scheduled",
                                Notes = $"Service duration set via Service Extend - Original: {duration} min",
                                IsActive = true,
                                DateCreated = DateTime.Now
                            };
                            db.Add(newAppointment);
                        }
                    }
                }

                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service durations updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating service durations: {ex.Message}";
            }

            return RedirectToAction("ServiceExtend", new { id = clientId });
        }
    }
}



