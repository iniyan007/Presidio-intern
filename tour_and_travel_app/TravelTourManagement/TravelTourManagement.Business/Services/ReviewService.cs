using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Reviews;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IPackagerRepository _packagerRepository;
    private readonly IMapper _mapper;

    public ReviewService(
        IReviewRepository reviewRepository,
        IBookingRepository bookingRepository,
        IPackageRepository packageRepository,
        IPackagerRepository packagerRepository,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _packageRepository = packageRepository;
        _packagerRepository = packagerRepository;
        _mapper = mapper;
    }

    public async Task<ReviewResponse> CreateReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Fetch booking
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        // 2. Verify ownership
        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You can only review your own bookings.");

        // 3. Verify status (must be Confirmed or Completed)
        if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException("You can only review a booking that is Confirmed or Completed.");

        // 4. Check for duplicates
        if (await _reviewRepository.HasUserReviewedBookingAsync(userId, request.BookingId, cancellationToken))
            throw new InvalidOperationException("You have already submitted a review for this booking.");

        // 5. Create Review Entity
        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            UserId = userId,
            PackageId = booking.PackageId,
            PackagerId = booking.Package?.PackagerId ?? Guid.Empty, // Fallback, though Include should ideally be used in GetByIdAsync
            OverallRating = request.OverallRating,
            AccommodationRating = request.AccommodationRating,
            TransportRating = request.TransportRating,
            FoodRating = request.FoodRating,
            GuideRating = request.GuideRating,
            ValueRating = request.ValueRating,
            Comment = request.Comment,
            IsVerifiedTraveler = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (review.PackagerId == Guid.Empty)
        {
            var package = await _packageRepository.GetByIdAsync(booking.PackageId, cancellationToken);
            if (package != null)
                review.PackagerId = package.PackagerId;
        }

        await _reviewRepository.AddAsync(review, cancellationToken);

        // 6. Update Package Average Rating
        var packageEntity = await _packageRepository.GetByIdAsync(review.PackageId, cancellationToken);
        if (packageEntity != null)
        {
            var newTotal = packageEntity.TotalReviews + 1;
            var newAvg = ((packageEntity.AvgRating * packageEntity.TotalReviews) + review.OverallRating) / newTotal;
            packageEntity.AvgRating = newAvg;
            packageEntity.TotalReviews = newTotal;
            await _packageRepository.UpdateAsync(packageEntity, cancellationToken);
        }

        // 7. Update Packager Average Rating
        var packagerEntity = await _packagerRepository.GetByIdAsync(review.PackagerId, cancellationToken);
        if (packagerEntity != null)
        {
            var newTotal = packagerEntity.TotalReviews + 1;
            var newAvg = ((packagerEntity.AvgRating * packagerEntity.TotalReviews) + review.OverallRating) / newTotal;
            packagerEntity.AvgRating = newAvg;
            packagerEntity.TotalReviews = newTotal;
            await _packagerRepository.UpdateAsync(packagerEntity, cancellationToken);
        }

        // We fetch the complete entity to get User info mapped properly
        var savedReview = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return _mapper.Map<ReviewResponse>(savedReview ?? review);
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetReviewsByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByPackageIdAsync(packageId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ReviewResponse>>(reviews);
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetReviewsByPackagerIdAsync(Guid packagerId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByPackagerIdAsync(packagerId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ReviewResponse>>(reviews);
    }
}
