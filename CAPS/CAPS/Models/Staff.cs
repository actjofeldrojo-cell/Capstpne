using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime DateHired { get; set; }

        public bool IsActive { get; set; }

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Method to check if staff is currently providing a service
        public bool IsCurrentlyInService()
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDate = now.Date;

            // Check if staff has any active appointment that is currently in progress
            var currentAppointment = Appointments?.FirstOrDefault(a => 
                a.IsActive && a.StaffId == StaffId &&
                a.Status != "Cancelled" &&
                a.Status != "Completed" &&
                a.Status != "No-Show" &&
                a.AppointmentDate.Date == currentDate &&
                currentTime >= a.AppointmentTime &&
                currentTime <= a.AppointmentTime.Add(TimeSpan.FromMinutes(a.Duration)));

            return currentAppointment != null;
        }

        // Method to check if staff is available for a specific time slot
        public bool IsAvailableForTimeSlot(DateTime appointmentDate, TimeSpan appointmentTime, int duration)
        {
            var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(duration));

            // Check if there are any conflicting appointments
            var conflictingAppointment = Appointments?.Any(a => 
                a.IsActive && 
                a.Status != "Cancelled" &&
                a.Status != "Completed" &&
                a.Status != "No-Show" &&
                a.AppointmentDate.Date == appointmentDate.Date &&
                a.AppointmentTime < appointmentEndTime &&
                a.AppointmentTime.Add(TimeSpan.FromMinutes(a.Duration)) > appointmentTime);

            return conflictingAppointment != true;
        }

        // Property to get current availability status
        public string AvailabilityStatus => IsCurrentlyInService() ? "In Service" : "Available";
    }
}
