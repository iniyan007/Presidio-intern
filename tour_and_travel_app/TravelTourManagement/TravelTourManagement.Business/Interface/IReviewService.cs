using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Reviews;

namespace TravelTourManagement.Business.Interface;

public interface IReviewService
{
    Task<ReviewResponse> CreateReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewResponse>> GetReviewsByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewResponse>> GetReviewsByPackagerIdAsync(Guid packagerId, CancellationToken cancellationToken = default);
}
