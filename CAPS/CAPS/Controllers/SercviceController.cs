using CAPS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPS.Controllers
{
    public class SercviceController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            return View(db.Services.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Service service)
        {
            if (ModelState.IsValid)
            {
                db.Services.Add(service);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(service);
        }
    }
}
