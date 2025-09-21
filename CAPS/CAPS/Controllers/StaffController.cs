using CAPS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPS.Controllers
{
    public class StaffController : GenericController
    {
        readonly AppDbContext db;
        public StaffController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            var staffWithAppointments = db.Staffs
                .Where(s => s.IsActive)
                .Select(s => new StaffWithAppointmentCount
                {
                    Staff = s,
                    TotalAppointments = db.Appointments.Count(a => a.StaffId == s.StaffId && a.IsActive),
                    UpcomingAppointments = db.Appointments.Count(a => a.StaffId == s.StaffId && a.IsActive && a.AppointmentDate >= DateTime.Today)
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
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    staffId = s.StaffId,
                    fullName = s.FullName,
                    expertise = s.Expertise
                })
                .ToList();

            return Json(activeStaff);
        }
    }

    public class StaffWithAppointmentCount
    {
        public Staff Staff { get; set; }
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
    }
}
