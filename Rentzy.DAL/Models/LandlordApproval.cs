using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
     public class LandlordApproval
        {
            [Key]
            public int Id { get; set; }
            [Required]
            public int LandlordId { get; set; }

            [ForeignKey(nameof(LandlordId))]
            public Landlord? Landlord { get; set; }

            public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
            [MaxLength(1000)]
            public string? DocumentUrl { get; set; }

            public int? ReviewedByAdminId { get; set; }

            public DateTime? ReviewedAt { get; set; }

            [Required]
            public int ApprovalStatusId { get; set; }

            [ForeignKey(nameof(ApprovalStatusId))]
            public ApprovalStatus? ApprovalStatus { get; set; }

            [MaxLength(2000)]
            public string? AdminNotes { get; set; }

            public bool IsDeleted { get; set; } = false;
     }
}
