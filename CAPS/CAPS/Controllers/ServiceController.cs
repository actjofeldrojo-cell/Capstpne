using CAPS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAPS.Controllers
{
    public class ServiceController : GenericController
    {
        readonly AppDbContext db;
        public ServiceController(AppDbContext db) { this.db = db; }

        public ActionResult Index()
        {
            var services = db.Services.Where(s => s.isActive).ToList();
            
            // If no services exist, create sample services
            if (!services.Any())
            {
                SeedSampleServices();
                services = db.Services.Where(s => s.isActive).ToList();
            }
            
            return View(services);
        }

        // User-facing gallery of services (picture + description only)
        public ActionResult Public(string? category)
        {
            var query = db.Services.Where(s => s.isActive);
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category == category);
            }
            var services = query.ToList();
            ViewBag.Categories = db.Services.Where(s => s.isActive)
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            ViewBag.SelectedCategory = category;
            return View(services);
        }

        // Update and Insert Viewing
        public ActionResult UpSert(int? id)
        {
            // Populate ViewBag with predefined service templates
            ViewBag.ServiceTemplates = new List<Service>
            {
                new Service
                {
                    Name = "Swedish Massage",
                    Description = "Classic relaxation massage using long strokes, kneading, and circular movements to promote relaxation and improve circulation.",
                    Category = "Massage",
                    Price = 250.00m,
                    Duration = 60
                },
                new Service
                {
                    Name = "Deep Tissue Massage",
                    Description = "Intensive massage targeting deep muscle layers to release chronic tension and improve mobility.",
                    Category = "Massage",
                    Price = 350.00m,
                    Duration = 90
                },
                new Service
                {
                    Name = "Hot Stone Massage",
                    Description = "Therapeutic massage using heated stones to relax muscles and provide deep warmth for ultimate relaxation.",
                    Category = "Massage",
                    Price = 400.00m,
                    Duration = 90
                },
                new Service
                {
                    Name = "Aromatherapy Massage",
                    Description = "Massage combined with essential oils to enhance relaxation and provide therapeutic benefits.",
                    Category = "Massage",
                    Price = 300.00m,
                    Duration = 60
                },
                new Service
                {
                    Name = "Classic Facial",
                    Description = "Basic facial treatment including cleansing, exfoliation, mask, and moisturizing for healthy, glowing skin.",
                    Category = "Facial",
                    Price = 200.00m,
                    Duration = 60
                },
                new Service
                {
                    Name = "Anti-Aging Facial",
                    Description = "Advanced facial treatment targeting fine lines and wrinkles with specialized serums and techniques.",
                    Category = "Facial",
                    Price = 350.00m,
                    Duration = 90
                },
                new Service
                {
                    Name = "Acne Treatment Facial",
                    Description = "Specialized facial treatment for acne-prone skin with deep cleansing and healing properties.",
                    Category = "Facial",
                    Price = 250.00m,
                    Duration = 75
                },
                new Service
                {
                    Name = "Body Scrub",
                    Description = "Exfoliating body treatment to remove dead skin cells and improve skin texture and tone.",
                    Category = "Body Treatment",
                    Price = 200.00m,
                    Duration = 45
                },
                new Service
                {
                    Name = "Mud Wrap",
                    Description = "Detoxifying body treatment using therapeutic mud to draw out impurities and nourish the skin.",
                    Category = "Body Treatment",
                    Price = 300.00m,
                    Duration = 60
                },
                new Service
                {
                    Name = "Classic Manicure",
                    Description = "Basic nail care including shaping, cuticle care, polish application, and hand massage.",
                    Category = "Nail Care",
                    Price = 150.00m,
                    Duration = 45
                },
                new Service
                {
                    Name = "Gel Manicure",
                    Description = "Long-lasting manicure with gel polish that provides chip-resistant color for up to two weeks.",
                    Category = "Nail Care",
                    Price = 250.00m,
                    Duration = 60
                },
                new Service
                {
                    Name = "Classic Pedicure",
                    Description = "Complete foot care including nail shaping, cuticle care, exfoliation, and polish application.",
                    Category = "Nail Care",
                    Price = 200.00m,
                    Duration = 60
                },
                new Service
                {
                    Name = "Luxury Pedicure",
                    Description = "Premium pedicure with extended foot massage, paraffin treatment, and premium polish.",
                    Category = "Nail Care",
                    Price = 350.00m,
                    Duration = 90
                },
                new Service
                {
                    Name = "Scalp Treatment",
                    Description = "Therapeutic scalp treatment to promote hair growth, reduce dandruff, and improve scalp health.",
                    Category = "Hair Care",
                    Price = 180.00m,
                    Duration = 45
                },
                new Service
                {
                    Name = "Reiki Session",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Energy Healing",
                    Price = 200.00m,
                    Duration = 60
                }
            };
            
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

        private void SeedSampleServices()
        {
            var sampleServices = new List<Service>
            {
                //A. MASSAGE
                new Service
                {
                    Name = "Swedish Massage",
                    Description = "Classic relaxation massage using long strokes, kneading, and circular movements to promote relaxation and improve circulation.",
                    Category = "Massage",
                    Price = 250.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Tenderbody Signature Massage",
                    Description = "Intensive massage targeting deep muscle layers to release chronic tension and improve mobility.",
                    Category = "Massage",
                    Price = 459.00m,
                    Duration = 90,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Hot Stone Body Massage",
                    Description = "Therapeutic massage using heated stones to relax muscles and provide deep warmth for ultimate relaxation.",
                    Category = "Massage",
                    Price = 700.00m,
                    Duration = 90,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Aroma Oil Body Massage",
                    Description = "Massage combined with essential oils to enhance relaxation and provide therapeutic benefits.",
                    Category = "Massage",
                    Price = 350.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Hot Stone Black Massage",
                    Description = "Basic facial treatment including cleansing, exfoliation, mask, and moisturizing for healthy, glowing skin.",
                    Category = "Massage",
                    Price = 500.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Four Hands Massage",
                    Description = "Advanced facial treatment targeting fine lines and wrinkles with specialized serums and techniques.",
                    Category = "Massage",
                    Price = 650.00m,
                    Duration = 90,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Pinoy Hilot",
                    Description = "Specialized facial treatment for acne-prone skin with deep cleansing and healing properties.",
                    Category = "Massage",
                    Price = 450.00m,
                    Duration = 90,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Foot Massage",
                    Description = "Exfoliating body treatment to remove dead skin cells and improve skin texture and tone.",
                    Category = "Massage",
                    Price = 250.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Thai Body Massage (Dry)",
                    Description = "Detoxifying body treatment using therapeutic mud to draw out impurities and nourish the skin.",
                    Category = "Massage",
                    Price = 300.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Thai Swedish Combo Massage (Back with Oil)",
                    Description = "Basic nail care including shaping, cuticle care, polish application, and hand massage.",
                    Category = "Massage",
                    Price = 300.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Siatsu Body Massage (Dry)",
                    Description = "Long-lasting manicure with gel polish that provides chip-resistant color for up to two weeks.",
                    Category = "Massage",
                    Price = 300.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Head & Back Massage",
                    Description = "Complete foot care including nail shaping, cuticle care, exfoliation, and polish application.",
                    Category = "Massage",
                    Price = 250.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Foot & Back Massage",
                    Description = "Premium pedicure with extended foot massage, paraffin treatment, and premium polish.",
                    Category = "Massage",
                    Price = 250.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Herbal Thai Balls Massage",
                    Description = "Therapeutic scalp treatment to promote hair growth, reduce dandruff, and improve scalp health.",
                    Category = "Massage",
                    Price = 450.00m,
                    Duration = 90,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Ventosa Massage",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Massage",
                    Price = 90.00m,
                    Duration = 600,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                //B. FACIAL, SKIN CARE & BODY TREATMENT
                new Service
                {
                    Name = "Cleansing Facial ",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial , Skin Care & Body Treatment",
                    Price = 250.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Deep Cleaning Facial w/ Brush, Vaccum, Laser",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 400.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Skin Polishing Facial (Normal, Dry & Oily Skin)",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 450.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Facial Detox (Normal, Dry & Oily Skin)",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 600.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Skin Whitening Facial",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 600.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Ultimate Anti Aging Facial (Normal, Dry & Oily Skin)",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 850.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Tenderbody Signature Facial (Normal, Dry & Oily Skin)",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 1050.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "V-Beauty Face Contour",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 950.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Wrinkles & Eyebag Reductiion",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 950.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Body Glow Treatment",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 650.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Tummy Body Contour Treatment",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 1050.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Varicose Reduction Treatment",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Facial, Skin Care & Body Treatment",
                    Price = 950.00m,
                    Duration = 60,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                //C. WAXING/THREADING
                new Service
                {
                    Name = "Upper Lip",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 200.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Lowwer Lip",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 200.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Under Arms",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 250.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Chin",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 200.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Half Legs",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 500.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Full Legs",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 800.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Eyebrows",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 200.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Arms",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 350.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Brazilian",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Waxing/Threading",
                    Price = 750.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                //D. PARAFFIN
                new Service
                {
                    Name = "Hands",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Parafin",
                    Price = 300.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Elbows",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Parafin",
                    Price = 250.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Feet",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Parafin",
                    Price = 400.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Back",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Parafin",
                    Price = 750.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                //E. OTHER SERVICES
                new Service
                {
                    Name = "Tenderbody Signature Body Scrub",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Other Services",
                    Price = 550.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Body Scrub",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Other Services",
                    Price = 350.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Foot Scrub",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Other Services",
                    Price = 350.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                },
                new Service
                {
                    Name = "Ear Candling",
                    Description = "Energy healing session to promote balance, reduce stress, and enhance overall well-being.",
                    Category = "Other Services",
                    Price = 350.00m,
                    Duration = 0,
                    isActive = true,
                    DateCreated = DateTime.Now
                }
            };

            foreach (var service in sampleServices)
            {
                db.Services.Add(service);
            }
            
            db.SaveChanges();
        }
    }
}
