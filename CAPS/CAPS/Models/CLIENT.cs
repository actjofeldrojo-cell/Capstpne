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

        // Client Preferences
        [StringLength(50)]
        [Display(Name = "Preferred Therapist Gender")]
        public string? PreferredTherapistGender { get; set; }

        [StringLength(20)]
        [Display(Name = "Massage Pressure Level")]
        public string? MassagePressureLevel { get; set; }

        [StringLength(50)]
        [Display(Name = "Music Preference")]
        public string? MusicPreference { get; set; }

        [StringLength(50)]
        [Display(Name = "Temperature Preference")]
        public string? TemperaturePreference { get; set; }

        [StringLength(200)]
        [Display(Name = "Comfort Item Preferences")]
        public string? ComfortItemPreferences { get; set; }

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
