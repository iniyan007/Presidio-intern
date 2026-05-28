using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IRepository<PackageSeasonalPricing, Guid> _seasonalPricingRepository;
    private readonly IUserRepository _userRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IPackageRepository packageRepository,
        IRepository<PackageSeasonalPricing, Guid> seasonalPricingRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _packageRepository = packageRepository;
        _seasonalPricingRepository = seasonalPricingRepository;
        _userRepository = userRepository;
    }

    public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, CancellationToken cancellationToken = default)
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

        // Check availability
        var totalTravelers = adultCount + childCount + request.InfantCount;
        if (totalTravelers > pricing.AvailableSlots)
            throw new InvalidOperationException($"Not enough slots available. Requested: {totalTravelers}, Available: {pricing.AvailableSlots}");
        
        if (package.CurrentBookings + totalTravelers > package.MaxCapacity)
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

        // Assuming a standard 5% platform fee and 10% tax for now
        decimal platformFeePercent = 5.0m;
        decimal platformFeeAmount = baseAmount * (platformFeePercent / 100m);
        decimal taxAmount = (baseAmount + platformFeeAmount) * 0.10m;
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

                if (t.AadharCardFile != null && t.AadharCardFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + t.AadharCardFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        t.AadharCardFile.CopyTo(fileStream); 
                    }
                    traveler.TravelDocuments.Add(new TravelDocument
                    {
                        DocumentType = "Aadhar Card",
                        FileName = uniqueFileName,
                        OriginalFilename = t.AadharCardFile.FileName,
                        FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                        FileSizeBytes = t.AadharCardFile.Length,
                        MimeType = t.AadharCardFile.ContentType,
                        UploadedAt = DateTime.UtcNow
                    });
                }

                if (t.PassportFile != null && t.PassportFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + t.PassportFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        t.PassportFile.CopyTo(fileStream);
                    }
                    traveler.TravelDocuments.Add(new TravelDocument
                    {
                        DocumentType = "Passport",
                        FileName = uniqueFileName,
                        OriginalFilename = t.PassportFile.FileName,
                        FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                        FileSizeBytes = t.PassportFile.Length,
                        MimeType = t.PassportFile.ContentType,
                        UploadedAt = DateTime.UtcNow
                    });
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


        // Update slots
        pricing.AvailableSlots -= totalTravelers;
        await _seasonalPricingRepository.UpdateAsync(pricing, cancellationToken);

        package.CurrentBookings += totalTravelers;
        await _packageRepository.UpdateAsync(package, cancellationToken);

        return new BookingResponse(
            booking.Id,
            booking.UserId,
            booking.PackageId,
            booking.BookingReference,
            booking.AdultCount,
            booking.ChildCount,
            booking.InfantCount,
            booking.TotalAmount,
            booking.PaidAmount,
            booking.PaymentStatus.ToString(),
            booking.TravelDate,
            booking.ReturnDate,
            booking.SpecialRequests,
            booking.BookedAt,
            booking.CancelledAt,
            booking.CancellationReason,
            booking.BookingTravelers.Select(t => new BookingTravelerResponse(
                t.Id,
                t.FullName,
                t.PassportNumber,
                t.DateOfBirth,
                t.Nationality,
                t.Age,
                t.Gender,
                t.MealPreference,
                t.AadharCardNumber,
                t.IsPrimary,
                t.TravelDocuments?.Select(d => new TravelDocumentResponse(d.Id, d.DocumentType, d.FilePath, d.FileName, d.UploadedAt)).ToList() ?? new List<TravelDocumentResponse>()
            )).ToList()
        );
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

        booking.Status = TravelTourManagement.DataAccess.Enums.BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        return new BookingResponse(
            booking.Id,
            booking.UserId,
            booking.PackageId,
            booking.BookingReference,
            booking.AdultCount,
            booking.ChildCount,
            booking.InfantCount,
            booking.TotalAmount,
            booking.PaidAmount,
            booking.PaymentStatus.ToString(),
            booking.TravelDate,
            booking.ReturnDate,
            booking.SpecialRequests,
            booking.BookedAt,
            booking.CancelledAt,
            booking.CancellationReason,
            booking.BookingTravelers.Select(t => new BookingTravelerResponse(
                t.Id,
                t.FullName,
                t.PassportNumber,
                t.DateOfBirth,
                t.Nationality,
                t.Age,
                t.Gender,
                t.MealPreference,
                t.AadharCardNumber,
                t.IsPrimary,
                t.TravelDocuments?.Select(d => new TravelDocumentResponse(d.Id, d.DocumentType, d.FilePath, d.FileName, d.UploadedAt)).ToList() ?? new List<TravelDocumentResponse>()
            )).ToList()
        );
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

        return bookings.Select(booking => new BookingResponse(
            booking.Id,
            booking.UserId,
            booking.PackageId,
            booking.BookingReference,
            booking.AdultCount,
            booking.ChildCount,
            booking.InfantCount,
            booking.TotalAmount,
            booking.PaidAmount,
            booking.PaymentStatus.ToString(),
            booking.TravelDate,
            booking.ReturnDate,
            booking.SpecialRequests,
            booking.BookedAt,
            booking.CancelledAt,
            booking.CancellationReason,
            booking.BookingTravelers.Select(t => new BookingTravelerResponse(
                t.Id,
                t.FullName,
                t.PassportNumber,
                t.DateOfBirth,
                t.Nationality,
                t.Age,
                t.Gender,
                t.MealPreference,
                t.AadharCardNumber,
                t.IsPrimary,
                t.TravelDocuments?.Select(d => new TravelDocumentResponse(d.Id, d.DocumentType, d.FilePath, d.FileName, d.UploadedAt)).ToList() ?? new List<TravelDocumentResponse>()
            )).ToList()
        )).ToList();
    }
}
