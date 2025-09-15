using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        // Personal Information
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(100)]
        public string? Age { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        //[EmailAddress]
        //[StringLength(100)]
        //public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
