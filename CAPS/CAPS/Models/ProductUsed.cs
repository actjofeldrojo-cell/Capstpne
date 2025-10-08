using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class ProductUsed
    {
        [Key]
        public int ProductUsedId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        public int? AppointmentId { get; set; }
        public int TransactionId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }


        [DataType(DataType.DateTime)]
        [Display(Name = "Date Used")]
        public DateTime DateUsed { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Modified")]
        public DateTime? DateModified { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Products Product { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }
        public virtual Transaction? Transaction { get; set; }
    }
}   
