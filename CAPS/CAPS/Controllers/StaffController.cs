using CAPS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPS.Controllers
{
    public class StaffController : Controller
    {
        readonly AppDbContext db;
        public StaffController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            return View(db.Staffs.Where(s => s.IsActive).ToList());
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


    }
}
