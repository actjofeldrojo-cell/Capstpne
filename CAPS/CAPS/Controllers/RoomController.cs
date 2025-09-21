using CAPS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPS.Controllers
{
    public class RoomController : GenericController
    {
        readonly AppDbContext db;
        public RoomController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            var rooms = db.Rooms.Where(r => r.IsAvailable).ToList();
            
            // If no rooms exist, create sample rooms
            if (!rooms.Any())
            {
                SeedSampleRooms();
                rooms = db.Rooms.Where(r => r.IsAvailable).ToList();
            }
            
            return View(rooms);
        }

        // Update and Insert Viewing
        public ActionResult UpSert(int? id)
        {
            // Populate ViewBag with predefined room types
            ViewBag.RoomTypes = new List<string>
            {
                "Massage Room",
                "Facial Room", 
                "VIP Suite",
                "Couples Room",
                "Treatment Room",
                "Relaxation Room",
                "Spa Suite",
                "Therapy Room"
            };
            
            return View(id == null ? new Room() { IsAvailable = true } : db.Rooms.FirstOrDefault(r => r.RoomId == id));
        }

        // Update and Insert logic (Add and Update to Database)
        [HttpPost]
        public ActionResult UpSert(Room room)
        {
            ModelState.Remove("DateCreated");
            if (!ModelState.IsValid) { return View(room); }
            
            room.DateCreated = DateTime.Now;

            // indicating that this room is new
            if (room.RoomId == 0)
            {
                db.Rooms.Add(room);
            }
            else // indicating that the room is existing
            {
                room.DateModified = DateTime.Now;
                db.Rooms.Update(room);
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Room");
        }

        public ActionResult Delete(int id)
        {
            Room room = db.Rooms.FirstOrDefault(r => r.RoomId == id);
            room.IsAvailable = false;

            db.Rooms.Update(room);
            db.SaveChanges();
            return RedirectToAction("Index", "Room");
        }

        private void SeedSampleRooms()
        {
            var sampleRooms = new List<Room>
            {
                new Room
                {
                    RoomNumber = "R001",
                    RoomType = "Massage Room",
                    IsAvailable = true,
                    DateCreated = DateTime.Now
                },
                new Room
                {
                    RoomNumber = "R002", 
                    RoomType = "Facial Room",
                    IsAvailable = true,
                    DateCreated = DateTime.Now
                },
                new Room
                {
                    RoomNumber = "R003",
                    RoomType = "VIP Suite",
                    IsAvailable = true,
                    DateCreated = DateTime.Now
                },
                new Room
                {
                    RoomNumber = "R004",
                    RoomType = "Couples Room",
                    IsAvailable = true,
                    DateCreated = DateTime.Now
                },
                new Room
                {
                    RoomNumber = "R005",
                    RoomType = "Treatment Room",
                    IsAvailable = true,
                    DateCreated = DateTime.Now
                },
                new Room
                {
                    RoomNumber = "R006",
                    RoomType = "Relaxation Room",
                    IsAvailable = true,
                    DateCreated = DateTime.Now
                }
            };

            foreach (var room in sampleRooms)
            {
                db.Rooms.Add(room);
            }
            
            db.SaveChanges();
        }
    }
}
