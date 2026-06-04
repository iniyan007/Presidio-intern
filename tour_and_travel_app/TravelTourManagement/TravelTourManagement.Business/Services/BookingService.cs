using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using Microsoft.AspNetCore.Http;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using Quartz;
using AutoMapper;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Business.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IRepository<PackageSeasonalPricing, Guid> _seasonalPricingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlatformConfigService _platformConfigService;
    private readonly IMapper _mapper;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IRepository<TravelDocument, Guid> _documentRepository;
    private readonly IPdfService _pdfService;
    private readonly INotificationService _notificationService;

    public BookingService(
        IPlatformConfigService platformConfigService,
        IMapper mapper,
        IBookingRepository bookingRepository,
        IPackageRepository packageRepository,
        IRepository<PackageSeasonalPricing, Guid> seasonalPricingRepository,
        IUserRepository userRepository,
        IRepository<TravelDocument, Guid> documentRepository,
        ISchedulerFactory schedulerFactory,
        IPdfService pdfService,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _packageRepository = packageRepository;
        _seasonalPricingRepository = seasonalPricingRepository;
        _userRepository = userRepository;
        _platformConfigService = platformConfigService;
        _mapper = mapper;
        _schedulerFactory = schedulerFactory;
        _documentRepository = documentRepository;
        _pdfService = pdfService;
        _notificationService = notificationService;
    }

    public async Task<byte[]> DownloadBookingTicketAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetWithFullDetailsAsync(bookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this booking.");

        if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException("Ticket can only be downloaded for confirmed bookings.");

        return _pdfService.GenerateBookingTicketPdf(booking);
    }

    public async Task CancelBookingAsync(Guid userId, Guid bookingId, CancelBookingRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this booking.");

        if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Refunded)
            throw new InvalidOperationException($"Booking cannot be cancelled because its current status is {booking.Status}.");

        // Calculate seat-consuming travelers
        var seatConsumingTravelers = booking.AdultCount + booking.ChildCount;

        // Restore slots in SeasonalPricing
        var pricing = await _seasonalPricingRepository.GetByIdAsync(booking.SeasonalPricingId, cancellationToken);
        if (pricing != null)
        {
            pricing.AvailableSlots += seatConsumingTravelers;
            await _seasonalPricingRepository.UpdateAsync(pricing, cancellationToken);
        }

        // Restore slots in Package
        var package = await _packageRepository.GetByIdAsync(booking.PackageId, cancellationToken);
        if (package != null)
        {
            package.CurrentBookings -= seatConsumingTravelers;
            // Prevent negative bookings just in case
            if (package.CurrentBookings < 0) package.CurrentBookings = 0;
            await _packageRepository.UpdateAsync(package, cancellationToken);
        }

        // Update Booking Status
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = request.CancellationReason;

        // Note: We leave PaymentStatus alone, or mark it as Refunded if Paid.
        // We will just let the Status handle the cancellation state.
        
        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        await _notificationService.SendNotificationAsync(
            booking.UserId, 
            "Booking Cancelled", 
            $"Your booking {booking.BookingReference} has been cancelled successfully.", 
            booking.Id, 
            TravelTourManagement.DataAccess.Enums.NotificationType.booking,
            cancellationToken);
    }

    public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, List<IFormFile>? documentFiles = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var package = await _packageRepository.GetByIdAsync(request.PackageId, cancellationToken);
        if (package == null || package.Status != TravelTourManagement.DataAccess.Enums.PackageStatus.Published)
            throw new InvalidOperationException("Package is not available for booking.");

        var pricing = await _seasonalPricingRepository.GetByIdAsync(request.SeasonalPricingId, cancellationToken);
        if (pricing == null || pricing.PackageId != package.Id || !pricing.IsActive)
            throw new InvalidOperationException("Pricing tier is not valid or active.");

        if (package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Honeymoon ||
            package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Private ||
            package.Type == TravelTourManagement.DataAccess.Enums.PackageType.Family)
        {
            var oneMonthFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
            if (request.TravelDate < oneMonthFromNow)
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException($"{package.Type} packages must be booked at least 1 month in advance.");
            }
        }

        
        
        // Passport Validation
        bool isIndia = package.Country?.Equals("India", StringComparison.OrdinalIgnoreCase) ?? false;
        if (!isIndia)
        {
            if (request.Travelers.Any(t => string.IsNullOrWhiteSpace(t.PassportNumber)))
                throw new InvalidOperationException("Passport Number is required for all travelers when traveling outside of India.");
        }

        // Calculate Adult and Child Counts based on Travelers list
        int adultCount = 0;
        int childCount = 0;
        foreach (var traveler in request.Travelers)
        {
            int age = traveler.Age ?? 
                     (traveler.DateOfBirth.HasValue ? DateTime.Today.Year - traveler.DateOfBirth.Value.Year : 20); // Default to adult if unknown
            
            if (age < 12) childCount++;
            else adultCount++;
        }

        // Check availability (Infants do not consume seats)
        var totalTravelers = adultCount + childCount + request.InfantCount;
        var seatConsumingTravelers = adultCount + childCount;
        
        if (seatConsumingTravelers > pricing.AvailableSlots)
            throw new InvalidOperationException($"Not enough slots available. Requested: {seatConsumingTravelers}, Available: {pricing.AvailableSlots}");
        
        if (package.CurrentBookings + seatConsumingTravelers > package.MaxCapacity)
            throw new InvalidOperationException("Package max capacity exceeded.");

        // Generate Booking Reference
        string reference;
        do
        {
            var randomString = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            reference = $"BKG-{DateTime.UtcNow.Year}-{randomString}";
        } while (await _bookingRepository.ReferenceExistsAsync(reference, cancellationToken));

        // Calculate Prices
        decimal baseAmount = (adultCount * pricing.BasePrice) + (childCount * (pricing.ChildPrice));
        if (pricing.DiscountPercent > 0)
        {
            baseAmount -= baseAmount * (pricing.DiscountPercent / 100m);
        }

        var platformConfig = await _platformConfigService.GetConfigAsync(cancellationToken);
        decimal platformFeePercent = platformConfig.PlatformFeePercent;
        decimal platformFeeAmount = baseAmount * (platformFeePercent / 100m);
        decimal taxAmount = (baseAmount + platformFeeAmount) * (platformConfig.GstPercent / 100m);
        decimal totalAmount = baseAmount + platformFeeAmount + taxAmount;

        var booking = new Booking
        {
            UserId = userId,
            PackageId = request.PackageId,
            SeasonalPricingId = request.SeasonalPricingId,
            BookingReference = reference,
            AdultCount = adultCount,
            ChildCount = childCount,
            InfantCount = request.InfantCount,
            TravelDate = request.TravelDate,
            ReturnDate = request.TravelDate.AddDays(package.DurationDays),
            SpecialRequests = request.SpecialRequests,
            PackagerBaseAmount = baseAmount,
            PlatformFeePercent = platformFeePercent,
            PlatformFeeAmount = platformFeeAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PaidAmount = 0, // Unpaid at creation
            Status = TravelTourManagement.DataAccess.Enums.BookingStatus.Pending,
            PaymentStatus = TravelTourManagement.DataAccess.Enums.PaymentStatus.Unpaid,
            BookedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingTravelers = request.Travelers.Select(t => {
                var traveler = new BookingTraveler
                {
                    FullName = t.FullName,
                    DateOfBirth = t.DateOfBirth,
                    Age = t.Age,
                    Gender = t.Gender,
                    MealPreference = t.MealPreference,
                    AadharCardNumber = t.AadharCardNumber,
                    PassportNumber = t.PassportNumber,
                    Nationality = t.Nationality,
                    IsPrimary = t.IsPrimary,
                    TravelDocuments = new List<TravelDocument>()
                };

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "travel_documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(t.AadharCardFileName) && documentFiles != null)
                {
                    var file = documentFiles.FirstOrDefault(f => f.FileName == t.AadharCardFileName);
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream); 
                        }
                        traveler.TravelDocuments.Add(new TravelDocument
                        {
                            DocumentType = "Aadhar Card",
                            FileName = uniqueFileName,
                            OriginalFilename = file.FileName,
                            FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                            FileSizeBytes = file.Length,
                            MimeType = file.ContentType,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                if (!string.IsNullOrEmpty(t.PassportFileName) && documentFiles != null)
                {
                    var file = documentFiles.FirstOrDefault(f => f.FileName == t.PassportFileName);
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        traveler.TravelDocuments.Add(new TravelDocument
                        {
                            DocumentType = "Passport",
                            FileName = uniqueFileName,
                            OriginalFilename = file.FileName,
                            FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                            FileSizeBytes = file.Length,
                            MimeType = file.ContentType,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                return traveler;
            }).ToList()
        };


        foreach (var traveler in booking.BookingTravelers)
        {
            foreach (var doc in traveler.TravelDocuments)
            {
                doc.Booking = booking;
            }
        }

        await _bookingRepository.AddAsync(booking, cancellationToken);


        // Update slots (only for seat-consuming travelers)
        pricing.AvailableSlots -= seatConsumingTravelers;
        await _seasonalPricingRepository.UpdateAsync(pricing, cancellationToken);

        package.CurrentBookings += seatConsumingTravelers;
        await _packageRepository.UpdateAsync(package, cancellationToken);

        // Schedule Quartz Job to cancel booking if payment is not completed within 5 minutes
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey("BookingTimeoutJob");
        var trigger = TriggerBuilder.Create()
            .ForJob(jobKey)
            .StartAt(DateTimeOffset.UtcNow.AddMinutes(5)) // Run in exactly 5 minutes
            .UsingJobData("BookingId", booking.Id.ToString())
            .Build();

        await scheduler.ScheduleJob(trigger, cancellationToken);

        await _notificationService.SendNotificationAsync(
            userId, 
            "Booking Request Submitted", 
            $"Your booking request for {package.Title} has been successfully submitted! Reference: {booking.BookingReference}", 
            booking.Id, 
            TravelTourManagement.DataAccess.Enums.NotificationType.booking,
            cancellationToken);

        return _mapper.Map<BookingResponse>(booking);
    }

    public async Task<BookingResponse> VerifyBookingAsync(Guid packagerUserId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetWithFullDetailsAsync(bookingId, cancellationToken);
        if (booking == null)
            throw new InvalidOperationException("Booking not found.");

        if (booking.Package.Packager.UserId != packagerUserId)
            throw new UnauthorizedAccessException("You are not authorized to verify this booking because you don't own the package.");

        if (booking.Status != TravelTourManagement.DataAccess.Enums.BookingStatus.DocumentUnderReview)
            throw new InvalidOperationException($"Booking is not in DocumentUnderReview state. Current status: {booking.Status}");

        var unverifiedDocuments = booking.TravelDocuments?.Where(d => d.Status != TravelTourManagement.DataAccess.Enums.DocumentStatus.Verified).ToList();
        if (unverifiedDocuments != null && unverifiedDocuments.Any())
        {
            throw new InvalidOperationException("Cannot confirm booking until all travel documents are verified.");
        }

        booking.Status = TravelTourManagement.DataAccess.Enums.BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        await _notificationService.SendNotificationAsync(
            booking.UserId, 
            "Booking Confirmed", 
            $"Great news! Your booking {booking.BookingReference} has been confirmed by the packager Check My Bookings to download your ticket!.", 
            booking.Id, 
            TravelTourManagement.DataAccess.Enums.NotificationType.booking,
            cancellationToken);

        return _mapper.Map<BookingResponse>(booking);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetMyBookingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IReadOnlyList<BookingResponse>>(bookings);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetBookingsByPackageIdAsync(Guid userId, string userRole, Guid packageId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetWithFullDetailsAsync(packageId, cancellationToken);
        if (package == null)
            throw new InvalidOperationException("Package not found.");

        // If user is Packager, they must own the package. If Admin, they can view any.
        if (userRole != "Admin" && package.Packager.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view bookings for this package.");

        var bookings = await _bookingRepository.GetByPackageIdAsync(packageId, cancellationToken);

        return _mapper.Map<IReadOnlyList<BookingResponse>>(bookings);
    }

    public async Task<TravelDocumentResponse> VerifyDocumentAsync(Guid packagerUserId, Guid documentId, VerifyDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
            throw new KeyNotFoundException("Document not found.");

        var booking = await _bookingRepository.GetByIdAsync(document.BookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Associated booking not found.");

        var package = await _packageRepository.GetWithFullDetailsAsync(booking.PackageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Associated package not found.");

        if (package.Packager.UserId != packagerUserId)
            throw new UnauthorizedAccessException("You are not authorized to verify documents for this package.");

        if (request.IsVerified)
        {
            document.Status = TravelTourManagement.DataAccess.Enums.DocumentStatus.Verified;
            document.VerifiedAt = DateTime.UtcNow;
            document.VerifiedBy = packagerUserId;
            document.RejectionReason = null;
        }
        else
        {
            document.Status = TravelTourManagement.DataAccess.Enums.DocumentStatus.Rejected;
            document.VerifiedAt = DateTime.UtcNow;
            document.VerifiedBy = packagerUserId;
            document.RejectionReason = request.RejectionReason;
        }

        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Check if all documents for this booking are now verified
        var fullBooking = await _bookingRepository.GetWithFullDetailsAsync(booking.Id, cancellationToken);
        if (fullBooking != null && fullBooking.Status == TravelTourManagement.DataAccess.Enums.BookingStatus.DocumentUnderReview)
        {
            var unverifiedDocs = fullBooking.TravelDocuments?.Where(d => d.Status != TravelTourManagement.DataAccess.Enums.DocumentStatus.Verified).ToList();
            if (unverifiedDocs == null || !unverifiedDocs.Any())
            {
                // All documents are verified, auto-confirm the booking
                fullBooking.Status = TravelTourManagement.DataAccess.Enums.BookingStatus.Confirmed;
                fullBooking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(fullBooking, cancellationToken);
            }
        }

        if (request.IsVerified)
        {
            await _notificationService.SendNotificationAsync(
                booking.UserId,
                "Document Verified",
                $"Your travel document ({document.DocumentType}) has been verified successfully.",
                booking.Id,
                TravelTourManagement.DataAccess.Enums.NotificationType.booking,
                cancellationToken);
        }
        else
        {
            await _notificationService.SendNotificationAsync(
                booking.UserId,
                "Document Rejected",
                $"Your travel document ({document.DocumentType}) has been rejected. Reason: {request.RejectionReason}",
                booking.Id,
                TravelTourManagement.DataAccess.Enums.NotificationType.booking,
                cancellationToken);
        }

        return _mapper.Map<TravelDocumentResponse>(document);
    }

    public async Task<TravelDocumentResponse> ReuploadDocumentAsync(Guid userId, Guid documentId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
            throw new KeyNotFoundException("Document not found.");

        var booking = await _bookingRepository.GetByIdAsync(document.BookingId, cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Associated booking not found.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to update this document.");

        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file.");

        // Delete old file
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "travel_documents");
        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));
        if (File.Exists(oldFilePath))
        {
            File.Delete(oldFilePath);
        }

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        document.FileName = uniqueFileName;
        document.FilePath = $"/uploads/travel_documents/{uniqueFileName}";
        document.OriginalFilename = file.FileName;
        document.FileSizeBytes = file.Length;
        document.MimeType = file.ContentType;
        document.UploadedAt = DateTime.UtcNow;
        document.Status = TravelTourManagement.DataAccess.Enums.DocumentStatus.Uploaded;
        document.VerifiedAt = null;
        document.VerifiedBy = null;
        document.RejectionReason = null;

        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Check if all documents for this booking are now verified
        var fullBooking = await _bookingRepository.GetWithFullDetailsAsync(booking.Id, cancellationToken);
        if (fullBooking != null && fullBooking.Status == TravelTourManagement.DataAccess.Enums.BookingStatus.DocumentUnderReview)
        {
            var unverifiedDocs = fullBooking.TravelDocuments?.Where(d => d.Status != TravelTourManagement.DataAccess.Enums.DocumentStatus.Verified).ToList();
            if (unverifiedDocs == null || !unverifiedDocs.Any())
            {
                // All documents are verified, auto-confirm the booking
                fullBooking.Status = TravelTourManagement.DataAccess.Enums.BookingStatus.Confirmed;
                fullBooking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(fullBooking, cancellationToken);
            }
        }

        return _mapper.Map<TravelDocumentResponse>(document);
    }
}
