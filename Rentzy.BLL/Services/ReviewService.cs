using Rentzy.BLL.DTOs;
using Rentzy.BLL.Exceptions;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using System;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class ReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IPropertyRepository _propertyRepository;

        public ReviewService(IReviewRepository reviewRepository, IPropertyRepository propertyRepository)
        {
            _reviewRepository = reviewRepository;
            _propertyRepository = propertyRepository;
        }

        /// <summary>
        /// Validates that the user has a completed transaction record before persisting the review.
        /// </summary>
        public async Task SubmitReviewAsync(ReviewDTO dto)
        {
            // 1. Verify authorization through business rules
            var hasCompleted = await _reviewRepository.HasCompletedBookingAsync(dto.TenantId, dto.PropertyId);
            
            if (!hasCompleted)
            {
                throw new ValidationException("Unauthorized to review this property. Only finalized tenancies can be reviewed.");
            }

            // 2. Map DTO to Domain Entity
            var review = new Review
            {
                PropertyId = dto.PropertyId,
                TenantId = dto.TenantId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Persist
            await _reviewRepository.AddReviewAsync(review);
        }

        public async Task<ReviewDTO> PrepareReviewFormAsync(int propertyId, int tenantId)
        {
            var hasCompleted = await _reviewRepository.HasCompletedBookingAsync(tenantId, propertyId);
            
            if (!hasCompleted)
            {
                throw new ValidationException("You cannot review a property without a completed booking.");
            }

            var property = await _propertyRepository.GetPropertyByIdAsync(propertyId);
            
            return new ReviewDTO
            {
                PropertyId = propertyId,
                PropertyTitle = property?.Title ?? "Unknown Property",
                TenantId = tenantId
            };
        }
    }
}
