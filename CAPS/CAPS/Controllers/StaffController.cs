using CAPS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAPS.Controllers
{
    public class StaffController : Controller
    {
        readonly AppDbContext db;
        public StaffController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            var staffWithAppointments = db.Staffs
                .Include(s => s.Appointments.Where(a => a.IsActive))
                .Where(s => s.IsActive)
                .Select(s => new StaffWithAppointmentCount
                {
                    Staff = s,
                    TotalAppointments = db.Appointments.Count(a => a.StaffId == s.StaffId && a.IsActive),
                    UpcomingAppointments = db.Appointments.Count(a => a.StaffId == s.StaffId && a.IsActive && a.AppointmentDate >= DateTime.Today),
                    IsCurrentlyInService = s.IsCurrentlyInService()
                })
                .ToList();
                
            return View(staffWithAppointments);
        }

        // Update and Insert Viewing
        public ActionResult UpSert(int? id)
        {
            return View(id == null ? new Staff() { IsActive = true } : db.Staffs.FirstOrDefault(s => s.StaffId == id));
        }

        [HttpPost]
        public ActionResult UpSert(Staff staff)
        {
            if (!ModelState.IsValid) { return View(staff); }
            staff.IsActive = true;
            if (staff.StaffId == 0)
            {
                // Create staff from bound data
                db.Staffs.Add(staff);
                db.SaveChanges();

            }
            else
            {
                db.Staffs.Update(staff);
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Staff");
        }

        public ActionResult Delete(int id)
        {
            //LIKE IsDeleted
            Staff staff = db.Staffs.FirstOrDefault(s => s.StaffId == id);
            staff.IsActive = false;

            db.Staffs.Update(staff);
            db.SaveChanges();
            return RedirectToAction("Index", "Staff");
        }

        // GET: Staff/GetActiveStaff (AJAX endpoint)
        [HttpGet]
        public JsonResult GetActiveStaff()
        {
            var activeStaff = db.Staffs
                .Include(s => s.Appointments.Where(a => a.IsActive))
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    staffId = s.StaffId,
                    fullName = s.FullName,
                    expertise = s.Expertise,
                    isCurrentlyInService = s.IsCurrentlyInService(),
                    availabilityStatus = s.AvailabilityStatus
                })
                .ToList();

            return Json(activeStaff);
        }

        // GET: Staff/CheckStaffAvailability (AJAX endpoint)
        [HttpGet]
        public JsonResult CheckStaffAvailability(int staffId, DateTime? appointmentDate = null, TimeSpan? appointmentTime = null, int? duration = null)
        {
            var staff = db.Staffs
                .Include(s => s.Appointments.Where(a => a.IsActive))
                .FirstOrDefault(s => s.StaffId == staffId && s.IsActive);

            if (staff == null)
            {
                return Json(new { available = false, message = "Staff not found", status = "Not Found" });
            }

            var isCurrentlyInService = staff.IsCurrentlyInService();
            var status = staff.AvailabilityStatus;
            var message = isCurrentlyInService ? "Staff is currently in service" : "Staff is available";

            // If specific time slot is provided, check for conflicts
            if (appointmentDate.HasValue && appointmentTime.HasValue && duration.HasValue)
            {
                var isAvailableForSlot = staff.IsAvailableForTimeSlot(appointmentDate.Value, appointmentTime.Value, duration.Value);
                message = isAvailableForSlot ? "Staff is available for this time slot" : "Staff is not available for this time slot";
                status = isAvailableForSlot ? "Available" : "Not Available";
            }

            return Json(new { 
                available = !isCurrentlyInService, 
                message = message, 
                status = status,
                isCurrentlyInService = isCurrentlyInService
            });
        }
    }

    public class StaffWithAppointmentCount
    {
        public Staff Staff { get; set; }
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public bool IsCurrentlyInService { get; set; }
    }
}
