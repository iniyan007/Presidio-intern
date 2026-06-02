using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using Microsoft.AspNetCore.Http;

namespace TravelTourManagement.Business.Interface;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, List<IFormFile>? documentFiles = null, CancellationToken cancellationToken = default);
    Task<BookingResponse> VerifyBookingAsync(Guid packagerUserId, Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingResponse>> GetBookingsByPackageIdAsync(Guid userId, string userRole, Guid packageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingResponse>> GetMyBookingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TravelDocumentResponse> VerifyDocumentAsync(Guid packagerUserId, Guid documentId, VerifyDocumentRequest request, CancellationToken cancellationToken = default);
    Task<TravelDocumentResponse> ReuploadDocumentAsync(Guid userId, Guid documentId, IFormFile file, CancellationToken cancellationToken = default);
}
