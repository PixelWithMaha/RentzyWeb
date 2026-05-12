using Rentzy.BLL.DTOs;
using Rentzy.BLL.Exceptions;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<ReviewDTO>> GetReviewsForPropertyAsync(int propertyId)
        {
            var reviews = await _reviewRepository.GetReviewsByPropertyIdAsync(propertyId);
            
            return reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                TenantId = r.TenantId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                TenantName = r.Tenant != null ? $"{r.Tenant.FirstName} {r.Tenant.LastName}" : "Unknown Tenant"
            }).ToList();
        }

        public async Task<Dictionary<int, (double AverageRating, int ReviewCount)>> GetReviewAggregatesAsync(IEnumerable<int> propertyIds)
        {
            return await _reviewRepository.GetReviewAggregatesAsync(propertyIds);
        }

        public async Task<ReviewDTO> GetReviewForEditAsync(int reviewId, int tenantId)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            
            if (review == null)
            {
                throw new ValidationException("Review not found.");
            }

            if (review.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this review.");
            }

            return new ReviewDTO
            {
                Id = review.Id,
                PropertyId = review.PropertyId,
                PropertyTitle = review.Property?.Title ?? "Unknown Property",
                TenantId = review.TenantId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task UpdateReviewAsync(ReviewDTO dto)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(dto.Id);
            
            if (review == null)
            {
                throw new ValidationException("Review no longer exists.");
            }

            if (review.TenantId != dto.TenantId)
            {
                throw new UnauthorizedAccessException("Unauthorized modification attempt.");
            }

            // Apply mutations
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.CreatedAt = DateTime.UtcNow; // Optional: update timestamp or keep original. Re-stamping typically follows "last modified" logic.

            await _reviewRepository.UpdateReviewAsync(review);
        }

        public async Task DeleteReviewAsync(int reviewId, int tenantId)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            
            if (review == null)
            {
                // Idempotent delete, if it doesn't exist then it's already deleted
                return; 
            }

            if (review.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Unauthorized deletion attempt.");
            }

            await _reviewRepository.DeleteReviewAsync(review);
        }

        public async Task<int> GetTotalReviewsCountAsync()
        {
            return await _reviewRepository.GetTotalReviewsCountAsync();
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsForLandlordAsync(int landlordId)
        {
            var reviews = await _reviewRepository.GetReviewsByLandlordIdAsync(landlordId);
            return MapToDTOs(reviews);
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsForAdminAsync()
        {
            var reviews = await _reviewRepository.GetAllReviewsAsync();
            return MapToDTOs(reviews);
        }

        public async Task AdminDeleteReviewAsync(int reviewId)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review != null)
            {
                await _reviewRepository.DeleteReviewAsync(review);
            }
        }

        private IEnumerable<ReviewDTO> MapToDTOs(IEnumerable<Review> reviews)
        {
            return reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                PropertyTitle = r.Property?.Title ?? "Unknown",
                TenantId = r.TenantId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                TenantName = r.Tenant != null ? $"{r.Tenant.FirstName} {r.Tenant.LastName}" : "Unknown User"
            }).ToList();
        }
    }
}
