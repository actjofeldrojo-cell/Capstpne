using System.Collections.Generic;
using CAPS.Models;
using Microsoft.EntityFrameworkCore;

namespace CAPS.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Service> Services { get; set; }
        public DbSet<Staff> Staffs { get; set; }
    }
}