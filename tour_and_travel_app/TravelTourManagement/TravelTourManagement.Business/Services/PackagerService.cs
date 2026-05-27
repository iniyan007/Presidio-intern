using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Packagers;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class PackagerService : IPackagerService
{
    private readonly IPackagerRepository _packagerRepository;
    private readonly IUserRepository _userRepository;

    public PackagerService(IPackagerRepository packagerRepository, IUserRepository userRepository)
    {
        _packagerRepository = packagerRepository;
        _userRepository = userRepository;
    }

    public async Task<PackagerResponse> ApplyToBecomePackagerAsync(Guid userId, ApplyPackagerRequest request, CancellationToken cancellationToken = default)
    {
        if (await _packagerRepository.ExistsByUserIdAsync(userId, cancellationToken))
        {
            throw new InvalidOperationException("You have already submitted a packager application or are already a packager.");
        }

        var packager = new Packager
        {
            UserId = userId,
            CompanyName = request.CompanyName,
            BusinessLicenseNo = request.BusinessLicenseNo,
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            WebsiteUrl = request.WebsiteUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalReviews = 0,
            AvgRating = 0
        };

        var createdPackager = await _packagerRepository.AddAsync(packager, cancellationToken);
        return MapToResponse(createdPackager);
    }

    public async Task<PackagerResponse> ApprovePackagerAsync(Guid packagerId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByIdAsync(packagerId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("Packager application not found.");
        }

        if (packager.ApprovedAt != null)
        {
            throw new InvalidOperationException("Packager is already approved.");
        }

        var adminUser = await _userRepository.GetByIdAsync(adminUserId, cancellationToken);
        if (adminUser == null)
        {
            throw new UnauthorizedAccessException("Admin user not found.");
        }

        packager.ApprovedBy = adminUserId;
        packager.ApprovedAt = DateTime.UtcNow;
        packager.UpdatedAt = DateTime.UtcNow;

        await _packagerRepository.UpdateAsync(packager, cancellationToken);
        await _packagerRepository.UpdateStatusRawAsync(packagerId, "approved", cancellationToken);
        return MapToResponse(packager);
    }

    public async Task<PackagerResponse> RejectPackagerAsync(Guid packagerId, Guid adminUserId, string reason, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByIdAsync(packagerId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("Packager application not found.");
        }

        if (packager.ApprovedAt != null)
        {
            throw new InvalidOperationException("Cannot reject an already approved packager. Deactivate them instead.");
        }

        var adminUser = await _userRepository.GetByIdAsync(adminUserId, cancellationToken);
        if (adminUser == null)
        {
            throw new UnauthorizedAccessException("Admin user not found.");
        }

        packager.DeactivatedAt = DateTime.UtcNow;
        packager.DeactivationReason = reason;
        packager.UpdatedAt = DateTime.UtcNow;

        await _packagerRepository.UpdateAsync(packager, cancellationToken);
        await _packagerRepository.UpdateStatusRawAsync(packagerId, "deactivated", cancellationToken);
        return MapToResponse(packager);
    }

    public async Task<IEnumerable<PackagerResponse>> GetPendingPackagersAsync(CancellationToken cancellationToken = default)
    {
        var pendingPackagers = await _packagerRepository.GetPendingApprovalAsync(cancellationToken);
        return pendingPackagers.Select(MapToResponse);
    }

    public async Task<PackagerResponse> GetMyPackagerStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("No packager application found for this user.");
        }
        return MapToResponse(packager);
    }

    private static PackagerResponse MapToResponse(Packager pk)
    {
        string status = "Pending";
        if (pk.DeactivatedAt != null)
        {
            status = pk.ApprovedAt == null ? "Rejected" : "Deactivated";
        }
        else if (pk.ApprovedAt != null)
        {
            status = "Approved";
        }

        return new PackagerResponse(
            pk.Id,
            pk.UserId,
            pk.CompanyName,
            pk.BusinessLicenseNo,
            pk.Description,
            pk.ContactEmail,
            pk.ContactPhone,
            pk.WebsiteUrl,
            status,
            pk.DeactivationReason,
            pk.AvgRating,
            pk.TotalReviews,
            pk.CreatedAt
        );
    }
}
