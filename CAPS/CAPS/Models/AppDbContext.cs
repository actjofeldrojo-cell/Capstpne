using System.Collections.Generic;
using CAPS.Models;

namespace CAPS.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Service> Services { get; set; }
        public DbSet<Staff> Staffs { get; set; }

        public AppDbContext() : base("DefaultConnection") { }

        internal void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}