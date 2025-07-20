using CAPS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CAPS.Controllers
{
    public class ClientController : Controller
    {
        readonly AppDbContext db;
        public ClientController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            return View(db.Clients.Where(c => c.IsActive).ToList());
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

        public ActionResult Delete(int id)
        {
            //LIKE IsDeleted
            Client client = db.Clients.FirstOrDefault(c => c.ClientId == id);
            client.IsActive = false;

            db.Clients.Update(client);
            db.SaveChanges();
            return RedirectToAction("Index", "Client");
        }
    }
}



