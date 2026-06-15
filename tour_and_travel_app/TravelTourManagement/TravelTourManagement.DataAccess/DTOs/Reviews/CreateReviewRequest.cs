using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Reviews;

public record CreateReviewRequest(
    [Required] Guid BookingId,
    [Required] [Range(1, 5, ErrorMessage = "Overall rating must be between 1 and 5.")] short OverallRating,
    [Range(1, 5, ErrorMessage = "Accommodation rating must be between 1 and 5.")] short? AccommodationRating,
    [Range(1, 5, ErrorMessage = "Transport rating must be between 1 and 5.")] short? TransportRating,
    [Range(1, 5, ErrorMessage = "Food rating must be between 1 and 5.")] short? FoodRating,
    [Range(1, 5, ErrorMessage = "Guide rating must be between 1 and 5.")] short? GuideRating,
    [Range(1, 5, ErrorMessage = "Value rating must be between 1 and 5.")] short? ValueRating,
    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")] string? Comment,
    List<string> MediaFilePaths
);
