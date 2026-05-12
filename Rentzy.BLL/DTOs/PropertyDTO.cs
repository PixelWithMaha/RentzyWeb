using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rentzy.BLL.DTOs
{
    public class PropertyDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Monthly rent must be greater than 0")]
        public int Rent { get; set; }

        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        public int PropertyTypeId { get; set; }
        public string PropertyTypeName { get; set; } = string.Empty;

        public int LandlordId { get; set; }
        public string LandlordName { get; set; } = "N/A"; // Flattened

        public int StatusId { get; set; }  // approval status
        public string? StatusName { get; set; } // optional: for easier display


        // Flatten bookings to just tenant names
        public List<string> TenantNames { get; set; } = new List<string>();

        public List<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public bool IsApproved { get; set; }

        // Added for aggregate review rendering
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();

        // Review eligibility for logged in tenant
        public bool IsReviewEligible { get; set; }
        public bool HasExistingReview { get; set; }
        public int? ExistingReviewId { get; set; }
    }
}
