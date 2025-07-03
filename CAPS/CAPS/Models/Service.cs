using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(0, 99999.99, ErrorMessage = "Price must be between 0 and 99999.99.")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateModified { get; set; }

        [Required]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
        public int Duration { get; set; }

        public bool isActive { get; set; }
    }
}
