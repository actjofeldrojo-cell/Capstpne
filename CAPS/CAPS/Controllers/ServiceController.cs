using CAPS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPS.Controllers
{
    public class ServiceController : Controller
    {
        readonly AppDbContext db;
        public ServiceController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            return View(db.Services.Where(s => s.isActive).ToList());
        }

        // Update and Insert Viewing
        public ActionResult UpSert(int? id)
        {
            return View(id == null ? new Service() { isActive = true } : db.Services.FirstOrDefault(s => s.ServiceId == id));
        }

        // Update and Insert logic (Add and Update to Database)
        [HttpPost]
        public ActionResult UpSert(Service service)
        {
            ModelState.Remove("DateCreated");
            if (!ModelState.IsValid) { return View(service); }
            
            service.DateCreated = DateTime.Now;

            // indicating that this service is new
            if (service.ServiceId == 0)
            {
                db.Services.Add(service);
            }
            else // indicating that the service is existing
            {
                service.DateModified = DateTime.Now;
                db.Services.Update(service);
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Service");
        }

        public ActionResult Delete(int id)
        {
            Service service = db.Services.FirstOrDefault(s => s.ServiceId == id);
            service.isActive = false;

            db.Services.Update(service);
            db.SaveChanges();
            return RedirectToAction("Index", "Service");
        }
    }
}
