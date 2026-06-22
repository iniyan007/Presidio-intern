using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Packagers;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using AutoMapper;
using TravelTourManagement.Business.Interface;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TravelTourManagement.Business.Services;

public class PackagerService : IPackagerService
{
    private readonly IPackagerRepository _packagerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public PackagerService(IPackagerRepository packagerRepository, IUserRepository userRepository, IMapper mapper, INotificationService notificationService)
    {
        _packagerRepository = packagerRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _notificationService = notificationService;
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
            AvgRating = 0,
            PackagerDocuments = new List<PackagerDocument>()
        };

        var currentDirectory = Directory.GetCurrentDirectory(); 
        var solutionDirectory = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
        var uploadDirectory = Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "Packagers", "Documents");
        
        if (!Directory.Exists(uploadDirectory))
        {
            Directory.CreateDirectory(uploadDirectory);
        }

        async Task ProcessDocumentAsync(IFormFile file, string documentType)
        {
            if (file == null || file.Length == 0) return;
            
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDirectory, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            packager.PackagerDocuments.Add(new PackagerDocument
            {
                DocumentType = documentType,
                FilePath = $"/uploads/packagers/documents/{uniqueFileName}",
                FileName = uniqueFileName,
                OriginalFilename = file.FileName,
                FileSizeBytes = file.Length,
                MimeType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            });
        }

        await ProcessDocumentAsync(request.PanDocument, "PAN");
        await ProcessDocumentAsync(request.GstDocument, "GST");
        await ProcessDocumentAsync(request.BusinessRegistration, "Registration");

        var createdPackager = await _packagerRepository.AddAsync(packager, cancellationToken);
        
        await _notificationService.SendNotificationAsync(
            userId,
            "Application Submitted",
            "Your application to become a Packager has been successfully submitted and is pending review.",
            createdPackager.Id,
            TravelTourManagement.DataAccess.Enums.NotificationType.system,
            cancellationToken);

        return _mapper.Map<PackagerResponse>(createdPackager);
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

        await _notificationService.SendNotificationAsync(
            packager.UserId,
            "Packager Application Approved",
            "Congratulations! Your application to become a Packager has been approved. You can now start creating and publishing packages.",
            packager.Id,
            TravelTourManagement.DataAccess.Enums.NotificationType.approval,
            cancellationToken);

        return _mapper.Map<PackagerResponse>(packager);
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

        await _notificationService.SendNotificationAsync(
            packager.UserId,
            "Packager Application Rejected",
            $"Your application to become a Packager was rejected. Reason: {reason}",
            packager.Id,
            TravelTourManagement.DataAccess.Enums.NotificationType.approval,
            cancellationToken);

        return _mapper.Map<PackagerResponse>(packager);
    }

    public async Task<IEnumerable<PackagerResponse>> GetPendingPackagersAsync(string? searchTerm = null, string? sortOrder = null, CancellationToken cancellationToken = default)
    {
        var pendingPackagers = await _packagerRepository.GetPendingApprovalAsync(searchTerm, sortOrder, cancellationToken);
        return _mapper.Map<IEnumerable<PackagerResponse>>(pendingPackagers);
    }

    public async Task<IEnumerable<PackagerResponse>> GetApprovedPackagersAsync(string? searchTerm = null, string? sortOrder = null, CancellationToken cancellationToken = default)
    {
        var approvedPackagers = await _packagerRepository.GetApprovedPackagersAsync(searchTerm, sortOrder, cancellationToken);
        return _mapper.Map<IEnumerable<PackagerResponse>>(approvedPackagers);
    }

    public async Task<IEnumerable<PackagerResponse>> GetDeactivatedPackagersAsync(string? searchTerm = null, string? sortOrder = null, CancellationToken cancellationToken = default)
    {
        var deactivatedPackagers = await _packagerRepository.GetDeactivatedPackagersAsync(searchTerm, sortOrder, cancellationToken);
        return _mapper.Map<IEnumerable<PackagerResponse>>(deactivatedPackagers);
    }

    public async Task<PackagerResponse> DeactivatePackagerAsync(Guid packagerId, Guid adminUserId, string reason, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByIdAsync(packagerId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("Packager not found.");
        }

        if (packager.ApprovedAt == null)
        {
            throw new InvalidOperationException("Cannot deactivate a packager that is not approved.");
        }

        if (packager.DeactivatedAt != null)
        {
            throw new InvalidOperationException("Packager is already deactivated.");
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

        await _notificationService.SendNotificationAsync(
            packager.UserId,
            "Packager Account Deactivated",
            $"Your packager account has been deactivated. Reason: {reason}",
            packager.Id,
            TravelTourManagement.DataAccess.Enums.NotificationType.system,
            cancellationToken);

        return _mapper.Map<PackagerResponse>(packager);
    }

    public async Task<PackagerResponse> ReactivatePackagerAsync(Guid packagerId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByIdAsync(packagerId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("Packager not found.");
        }

        if (packager.DeactivatedAt == null)
        {
            throw new InvalidOperationException("Packager is not deactivated.");
        }

        var adminUser = await _userRepository.GetByIdAsync(adminUserId, cancellationToken);
        if (adminUser == null)
        {
            throw new UnauthorizedAccessException("Admin user not found.");
        }

        packager.DeactivatedAt = null;
        packager.DeactivationReason = null;
        packager.UpdatedAt = DateTime.UtcNow;

        await _packagerRepository.UpdateAsync(packager, cancellationToken);
        await _packagerRepository.UpdateStatusRawAsync(packagerId, "approved", cancellationToken);

        await _notificationService.SendNotificationAsync(
            packager.UserId,
            "Packager Account Reactivated",
            "Great news! Your packager account has been reactivated and you can now manage your packages.",
            packager.Id,
            TravelTourManagement.DataAccess.Enums.NotificationType.system,
            cancellationToken);

        return _mapper.Map<PackagerResponse>(packager);
    }

    public async Task<PackagerResponse> GetMyPackagerStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("No packager application found for this user.");
        }
        return _mapper.Map<PackagerResponse>(packager);
    }

    public async Task<TravelTourManagement.DataAccess.DTOs.PagedResponse<PublicPackagerResponse>> GetPublicPackagersAsync(PackagerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var (packagers, totalCount) = await _packagerRepository.SearchPublicPackagersAsync(request.SearchTerm, request.PageNumber, request.PageSize, cancellationToken);
        
        var responseItems = _mapper.Map<List<PublicPackagerResponse>>(packagers);

        return new TravelTourManagement.DataAccess.DTOs.PagedResponse<PublicPackagerResponse>(responseItems, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<PublicPackagerResponse> GetPublicPackagerByNameAsync(string packagerName, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByCompanyNameAsync(packagerName, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("Packager not found.");
        }
        return _mapper.Map<PublicPackagerResponse>(packager);
    }

    public async Task<IEnumerable<PackagerDocumentResponse>> GetPackagerDocumentsAsync(Guid packagerId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetWithDocumentsAsync(packagerId, cancellationToken);
        if (packager == null)
        {
            throw new KeyNotFoundException("Packager not found.");
        }

        return packager.PackagerDocuments.Select(doc => new PackagerDocumentResponse
        {
            Id = doc.Id,
            DocumentType = doc.DocumentType,
            FileName = doc.FileName,
            OriginalFilename = doc.OriginalFilename,
            FileSizeBytes = doc.FileSizeBytes,
            MimeType = doc.MimeType,
            UploadedAt = doc.UploadedAt,
            FileUrl = $"/api/Admin/packagers/documents/{doc.FileName}"
        });
    }
}
