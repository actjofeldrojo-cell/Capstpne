using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPS.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int? StaffId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Credit Card, Debit Card, Online Payment, etc.

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Completed"; // Completed, Pending, Cancelled, Refunded

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Discount and tax properties
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountAmount { get; set; } = 0.00m;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountPercentage { get; set; } = 0.00m;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxAmount { get; set; } = 0.00m;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal TaxPercentage { get; set; } = 0.00m;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        // Receipt properties
        [StringLength(100)]
        public string? ReceiptNumber { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        // Foreign key navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; }

        // Navigation property for products used in this transaction
        public virtual ICollection<ProductUsed> ProductsUsed { get; set; } = new List<ProductUsed>();

        // Computed property for total amount
        public void CalculateTotal()
        {
            decimal subtotal = Amount;
            
            // Apply discount
            if (DiscountPercentage > 0)
            {
                DiscountAmount = subtotal * (DiscountPercentage / 100);
                subtotal -= DiscountAmount;
            }
            else if (DiscountAmount > 0)
            {
                subtotal -= DiscountAmount;
            }
            
            // Apply tax
            if (TaxPercentage > 0)
            {
                TaxAmount = subtotal * (TaxPercentage / 100);
                subtotal += TaxAmount;
            }
            else if (TaxAmount > 0)
            {
                subtotal += TaxAmount;
            }
            
            TotalAmount = subtotal;
        }
    }
}
