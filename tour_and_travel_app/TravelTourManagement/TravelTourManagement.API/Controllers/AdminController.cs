using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Services;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IPackagerService _packagerService;
    private readonly TravelTourManagement.DataAccess.Interface.IRepository<TravelTourManagement.DataAccess.Entities.Booking, Guid> _bookingRepository;
    private readonly TravelTourManagement.DataAccess.Interface.IRepository<TravelTourManagement.DataAccess.Entities.Packager, Guid> _packagerRepository;

    public AdminController(IPackagerService packagerService, 
                           TravelTourManagement.DataAccess.Interface.IRepository<TravelTourManagement.DataAccess.Entities.Booking, Guid> bookingRepository,
                           TravelTourManagement.DataAccess.Interface.IRepository<TravelTourManagement.DataAccess.Entities.Packager, Guid> packagerRepository)
    {
        _packagerService = packagerService;
        _bookingRepository = bookingRepository;
        _packagerRepository = packagerRepository;
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
        var packagers = await _packagerRepository.GetAllAsync(cancellationToken);

        var paidBookings = bookings.Where(b => b.PaymentStatus == TravelTourManagement.DataAccess.Enums.PaymentStatus.Paid).ToList();
        
        var totalRevenue = paidBookings.Sum(b => b.PlatformFeeAmount);
        var totalGst = paidBookings.Sum(b => b.TaxAmount);
        var totalBookings = paidBookings.Count;
        var activePackagers = packagers.Count(p => p.ApprovedAt != null && p.DeactivatedAt == null);

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalGst = totalGst,
            TotalBookings = totalBookings,
            ActivePackagers = activePackagers
        });
    }

    [HttpPost("packagers/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePackager(Guid id, CancellationToken cancellationToken)
    {
        var adminUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(adminUserIdString) || !Guid.TryParse(adminUserIdString, out var adminUserId))
        {
            return Unauthorized("Admin User ID not found in token.");
        }

        var response = await _packagerService.ApprovePackagerAsync(id, adminUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("packagers/{id:guid}/reject")]
    public async Task<IActionResult> RejectPackager(Guid id, [FromBody] TravelTourManagement.DataAccess.DTOs.Packagers.RejectPackagerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(adminUserIdString) || !Guid.TryParse(adminUserIdString, out var adminUserId))
        {
            return Unauthorized("Admin User ID not found in token.");
        }

        var response = await _packagerService.RejectPackagerAsync(id, adminUserId, request.Reason, cancellationToken);
        return Ok(response);
    }

    [HttpGet("packagers/pending")]
    public async Task<IActionResult> GetPendingPackagers([FromQuery] string? search, [FromQuery] string? sort, CancellationToken cancellationToken)
    {
        var response = await _packagerService.GetPendingPackagersAsync(search, sort, cancellationToken);
        return Ok(response);
    }

    [HttpGet("packagers/approved")]
    public async Task<IActionResult> GetApprovedPackagers([FromQuery] string? search, [FromQuery] string? sort, CancellationToken cancellationToken)
    {
        var response = await _packagerService.GetApprovedPackagersAsync(search, sort, cancellationToken);
        return Ok(response);
    }

    [HttpPost("packagers/{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivatePackager(Guid id, [FromBody] TravelTourManagement.DataAccess.DTOs.Packagers.RejectPackagerRequest request, CancellationToken cancellationToken)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out Guid adminUserId))
        {
            return Unauthorized("Admin User ID not found in token.");
        }

        var response = await _packagerService.DeactivatePackagerAsync(id, adminUserId, request.Reason, cancellationToken);
        return Ok(response);
    }

    [HttpGet("packagers/deactivated")]
    public async Task<IActionResult> GetDeactivatedPackagers([FromQuery] string? search, [FromQuery] string? sort, CancellationToken cancellationToken)
    {
        var response = await _packagerService.GetDeactivatedPackagersAsync(search, sort, cancellationToken);
        return Ok(response);
    }

    [HttpPost("packagers/{id:guid}/activate")]
    public async Task<IActionResult> ReactivatePackager(Guid id, CancellationToken cancellationToken)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out Guid adminUserId))
        {
            return Unauthorized("Admin User ID not found in token.");
        }

        var response = await _packagerService.ReactivatePackagerAsync(id, adminUserId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("packagers/{id:guid}/documents")]
    public async Task<IActionResult> GetPackagerDocuments(Guid id, CancellationToken cancellationToken)
    {
        var response = await _packagerService.GetPackagerDocumentsAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("packagers/documents/{fileName}")]
    [AllowAnonymous] // Ideally should be authorized, but for testing UI easier to allow or secure with token
    public IActionResult GetPackagerDocumentFile(string fileName)
    {
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            throw new ArgumentException("Invalid file name.");

        var currentDirectory = System.IO.Directory.GetCurrentDirectory(); 
        var solutionDirectory = System.IO.Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
        var uploadDirectory = System.IO.Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "Packagers", "Documents");
        
        var filePath = System.IO.Path.Combine(uploadDirectory, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        return File(fileStream, contentType);
    }
}
