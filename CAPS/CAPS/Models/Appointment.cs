using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ServiceId { get; set; }


        public int? StaffId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes.")]
        [Display(Name = "Duration (minutes)")]
        public int Duration { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Cancelled, No-Show

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Range(0.01, 99999.99, ErrorMessage = "Cost must be between 0.01 and 99999.99.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Cost")]
        public decimal? Cost { get; set; }


        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? DateModified { get; set; }

        [StringLength(100, ErrorMessage = "Cancellation reason cannot exceed 100 characters.")]
        public string? CancellationReason { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CancellationDate { get; set; }

        [StringLength(100, ErrorMessage = "Cancelled by cannot exceed 100 characters.")]
        public string? CancelledBy { get; set; }

        // Navigation Properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; }
    }
}
