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
            var clients = await db.Clients
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Where(c => c.IsActive)
                .ToListAsync();

            // Filter out clients who have completed payments (have transactions with "Completed" status)
            var clientsWithCompletedPayments = await db.Transactions
                .Where(t => t.IsActive && t.Status == "Completed")
                .Select(t => t.ClientId)
                .Distinct()
                .ToListAsync();

            // Only show clients who don't have completed payments
            var filteredClients = clients
                .Where(c => !clientsWithCompletedPayments.Contains(c.ClientId))
                .ToList();

            return View(filteredClients);
        }

        //[HttpPost]
        //public async Task<IActionResult> MarkAsPaid(int clientId)
        //{
        //    var client = await db.Clients
        //        .Include(c => c.Appointments)
        //        .FirstOrDefaultAsync(c => c.ClientId == clientId);

        //    if (client == null)
        //        return Json(new { success = false, message = "Client not found." });

        //    foreach (var appt in client.Appointments.Where(a => a.IsActive))
        //    {
        //        appt.IsPaid = true;
        //    }

        //    await db.SaveChangesAsync();
        //    return Json(new { success = true, message = "Payment recorded and client removed from ready list." });
        //}



        // Update and Insert Viewing
        public ActionResult UpSert(int? id)
        {
            return View(id == null ? new Client() { IsActive = true } : db.Clients.FirstOrDefault(c => c.ClientId == id));
        }

        [HttpPost]
        public ActionResult UpSert(Client client, string[] ComfortItemPreferences)
        {
            // Handle checkbox values for comfort items
            if (ComfortItemPreferences != null && ComfortItemPreferences.Length > 0)
            {
                client.ComfortItemPreferences = string.Join(", ", ComfortItemPreferences);
            }

            if (!ModelState.IsValid) { return View(client); }

            bool isNewClient = client.ClientId == 0;

            if (isNewClient)
            {
                // Check if client with same phone number already exists
                var existingClient = db.Clients
                    .FirstOrDefault(c => c.PhoneNumber == client.PhoneNumber && c.IsActive);

                if (existingClient != null)
                {
                    // Client already exists - this is a duplicate registration
                    // Set flag to show retention modal
                    ViewBag.ShowRetentionModal = true;
                    ViewBag.ExistingClient = existingClient;
                    ViewBag.IsDuplicateRegistration = true;
                    return View(client);
                }

                // Create Client from bound data
                client.IsActive = true;
                client.DateRegistered = DateTime.Now;
                db.Clients.Add(client);
            }
            else
            {
                db.Clients.Update(client);
            }

            db.SaveChanges();

            TempData["SuccessMessage"] = "Client updated successfully!";
            return RedirectToAction("Index", "Client");
        }

        // Handle retention survey submission
        [HttpPost]
        public ActionResult SubmitRetentionSurvey(int clientId, int satisfactionRating, string feedback, string improvementSuggestions)
        {
            try
            {
                // Here you could save the retention survey data to a database table
                // For now, we'll just log it and show a success message

                // You could create a ClientRetentionSurvey model and save it:
                // var survey = new ClientRetentionSurvey
                // {
                //     ClientId = clientId,
                //     SatisfactionRating = satisfactionRating,
                //     Feedback = feedback,
                //     ImprovementSuggestions = improvementSuggestions,
                //     SurveyDate = DateTime.Now
                // };
                // db.ClientRetentionSurveys.Add(survey);
                // db.SaveChanges();

                TempData["SuccessMessage"] = "Thank you for your feedback! We appreciate your input and will use it to improve our services.";
                return Json(new { success = true, message = "Survey submitted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error submitting survey: {ex.Message}" });
            }
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

                client.IsActive = false;
                db.Clients.Update(client);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Client {client.FirstName} {client.LastName} has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting client: {ex.Message}";
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

            // Get client's current service durations (if any exist in appointments)
            var clientAppointments = await db.Appointments
                .Include(a => a.Service)
                .Where(a => a.ClientId == id && a.IsActive)
                .ToListAsync();

            // Get all active services
            var allServices = await db.Services.Where(s => s.isActive && clientAppointments.Select(q => q.ServiceId).Contains(s.ServiceId)).ToListAsync();

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

        // Get Services for In-Service modal
        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var services = await db.Services
                .Where(s => s.isActive)
                .Select(s => new {
                    serviceId = s.ServiceId,
                    name = s.Name,
                    price = s.Price,
                    duration = s.Duration
                })
                .ToListAsync();

            return Json(services);
        }

        // Get Rooms for In-Service modal
        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await db.Rooms
                .Where(r => r.IsAvailable)
                .Select(r => new {
                    roomId = r.RoomId,
                    roomNumber = r.RoomNumber,
                    roomType = r.RoomType
                })
                .ToListAsync();

            return Json(rooms);
        }

        // Get Staff for In-Service modal
        [HttpGet]
        public async Task<IActionResult> GetStaff()
        {
            var staffWithAppointments = db.Staffs
               .Include(s => s.Appointments.Where(a => a.IsActive))
               .Where(s => s.IsActive)
               .Select(s => new
               {
                   staffId = s.StaffId,
                   fullName = s.FullName,
                   expertise = s.Expertise,
                   IsCurrentlyInService = s.IsCurrentlyInService()
               })
               .ToList();

            staffWithAppointments = staffWithAppointments.Where(q => !q.IsCurrentlyInService).ToList();
            return Json(staffWithAppointments);

            //var staff = await db.Staffs
            //    .Where(s => s.IsActive && s.AvailabilityStatus == "Available")
            //    .Select(s => new {
            //        staffId = s.StaffId,
            //        fullName = s.FullName,
            //        expertise = s.Expertise
            //    })
            //    .ToListAsync();s

            //return Json(staff);
        }

        // Put Client In-Service
        [HttpPost]
        public async Task<IActionResult> InService(int clientId, int[] selectedServices, int selectedRoom, int selectedStaff, string serviceNotes = "")
        {
            try
            {
                var client = await db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found." });
                }

                var room = await db.Rooms.FirstOrDefaultAsync(r => r.RoomId == selectedRoom);
                if (room == null)
                {
                    return Json(new { success = false, message = "Room not found." });
                }

                var staff = await db.Staffs.FirstOrDefaultAsync(s => s.StaffId == selectedStaff);
                if (staff == null)
                {
                    return Json(new { success = false, message = "Staff member not found." });
                }

                if (selectedServices == null || selectedServices.Length == 0)
                {
                    return Json(new { success = false, message = "Please select at least one service." });
                }

                // Create appointments for each selected service
                foreach (var serviceId in selectedServices)
                {
                    var service = await db.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
                    if (service != null)
                    {
                        var appointment = new Appointment
                        {
                            ClientId = clientId,
                            ServiceId = serviceId,
                            Duration = service.Duration,
                            Cost = service.Price,
                            AppointmentDate = DateTime.Today,
                            AppointmentTime = DateTime.Now.TimeOfDay,
                            Status = "In-Service",
                            Notes = $"Room: {room.RoomNumber} | Staff: {staff.FullName} | {serviceNotes}",
                            IsActive = true,
                            DateCreated = DateTime.Now,
                            StaffId = selectedStaff
                        };
                        
                        db.Appointments.Add(appointment);
                    }
                }

                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Client {client.FirstName} {client.LastName} has been put in-service successfully!";

                return Json(new { success = true, message = "Client put in-service successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error putting client in-service: {ex.Message}" });
            }
        }
    }
}



