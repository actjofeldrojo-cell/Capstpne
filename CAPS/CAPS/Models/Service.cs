using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        [Display(Name = "Service Name")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Service category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Service price is required.")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Modified")]
        public DateTime? DateModified { get; set; }

        [Required(ErrorMessage = "Service duration is required.")]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
        [Display(Name = "Duration (minutes)")]
        public int Duration { get; set; }

        [Display(Name = "Active Status")]
        public bool isActive { get; set; }
    }
}
