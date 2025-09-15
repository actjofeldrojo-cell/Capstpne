using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Room number is required.")]
        [StringLength(20, ErrorMessage = "Room number cannot exceed 20 characters.")]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Room type is required.")]
        [StringLength(50, ErrorMessage = "Room type cannot exceed 50 characters.")]
        [Display(Name = "Room Type")]
        public string RoomType { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Modified")]
        public DateTime? DateModified { get; set; }

        // Navigation Properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
