using System.ComponentModel.DataAnnotations;

namespace CAPS.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Expertise { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime DateHired { get; set; }

        public bool IsActive { get; set; }
    }
}
