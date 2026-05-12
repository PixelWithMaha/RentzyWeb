using System;
using System.ComponentModel.DataAnnotations;

namespace Rentzy.BLL.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        
        public string? PropertyTitle { get; set; }

        public int TenantId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string Comment { get; set; } = string.Empty;

        // Added for display
        public string? TenantName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
