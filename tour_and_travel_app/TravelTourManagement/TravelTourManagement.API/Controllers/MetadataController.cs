using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;
using System.Threading.Tasks;
using System.Collections.Generic;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.DTOs.Packages;
using System.IO;

namespace TravelTourManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IPackageRepository _packageRepository;
    private readonly ApplicationDbContext _context;

    public MetadataController(IPackageRepository packageRepository, ApplicationDbContext context)
    {
        _packageRepository = packageRepository;
        _context = context;
    }

    /// <summary>
    /// Retrieves all the predefined Enums for frontend dropdown population.
    /// </summary>
    [HttpGet("enums")]
    [AllowAnonymous]
    public IActionResult GetEnums()
    {
        return Ok(new
        {
            PackageTypes = Enum.GetNames(typeof(PackageType)),
            PackageStatuses = Enum.GetNames(typeof(PackageStatus)),
            InclusionTypes = Enum.GetNames(typeof(InclusionType)),
            MediaCategories = Enum.GetNames(typeof(MediaCategory)),
            MealTypes = Enum.GetNames(typeof(MealType)),
            TransportModes = Enum.GetNames(typeof(TransportMode)),
            DaySessions = Enum.GetNames(typeof(DaySession)),
            BookingStatuses = Enum.GetNames(typeof(BookingStatus)),
            PaymentStatuses = Enum.GetNames(typeof(PaymentStatus))
        });
    }

    /// <summary>
    /// Retrieves a combined list of predefined standard countries and any dynamically added countries from the database.
    /// </summary>
    [HttpGet("countries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCountries(System.Threading.CancellationToken cancellationToken)
    {
        var standardCountries = new List<string> {
    "India",
    "United States",
    "United Kingdom",
    "Australia",
    "Canada",
    "Singapore",
    "Malaysia",
    "United Arab Emirates",
    "Thailand",
    "Maldives",
    "Sri Lanka",
    "France",
    "Germany",
    "Italy",
    "Switzerland",
    "Japan",
    "China",
    "South Korea",
    "Indonesia",
    "Vietnam",
    "Philippines",
    "Nepal",
    "Bhutan",
    "Bangladesh",
    "Pakistan",
    "New Zealand",
    "Turkey",
    "Saudi Arabia",
    "Qatar",
    "Oman",
    "Kuwait",
    "Bahrain",
    "Egypt",
    "South Africa",
    "Kenya",
    "Mauritius",
    "Russia",
    "Ukraine",
    "Netherlands",
    "Belgium",
    "Austria",
    "Spain",
    "Portugal",
    "Greece",
    "Norway",
    "Sweden",
    "Denmark",
    "Finland",
    "Ireland",
    "Poland",
    "Czech Republic",
    "Hungary",
    "Romania",
    "Croatia",
    "Serbia",
    "Bulgaria",
    "Mexico",
    "Brazil",
    "Argentina",
    "Chile",
    "Peru",
    "Colombia"
};
        var dynamicCountries = await _packageRepository.GetDistinctCountriesAsync(cancellationToken);
        
        var combined = standardCountries.Union(dynamicCountries).OrderBy(c => c).ToList();
        return Ok(combined);
    }

    /// <summary>
    /// Retrieves a combined list of predefined standard destinations and any dynamically added destinations from the database.
    /// Supports an optional 'country' query parameter to filter destinations by a specific country.
    /// </summary>
    [HttpGet("destinations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDestinations([FromQuery] string? country, System.Threading.CancellationToken cancellationToken)
    {
        var standardDestinationsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Kerala", "India" }, { "Goa", "India" }, { "Delhi", "India" },
            { "Bali", "Indonesia" }, { "Paris", "France" }, { "London", "UK" },
            { "New York", "USA" }, { "Dubai", "UAE" }, { "Sydney", "Australia" },
            { "Rome", "Italy" }, { "Tokyo", "Japan" }, { "Male", "Maldives" },
            { "Colombo", "Sri Lanka" }
        };

        IEnumerable<string> standardDestinations = standardDestinationsMap.Keys;

        if (!string.IsNullOrWhiteSpace(country))
        {
            standardDestinations = standardDestinationsMap
                .Where(kvp => string.Equals(kvp.Value, country, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key);
        }

        var dynamicDestinations = await _packageRepository.GetDistinctDestinationsAsync(country, cancellationToken);
        
        var combined = standardDestinations.Union(dynamicDestinations).OrderBy(d => d).ToList();
        return Ok(combined);
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> Seed(System.Threading.CancellationToken cancellationToken)
    {
        // Apply migrations automatically (gracefully catch if database is already migrated)
        try
        {
            await _context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Skip migration if already set up
        }

        // 1. Ensure platform config exists
        var hasConfig = await _context.PlatformConfigs.AnyAsync(cancellationToken);
        if (!hasConfig)
        {
            _context.PlatformConfigs.Add(new PlatformConfig
            {
                PlatformFeePercent = 5.00m,
                GstPercent = 18.00m,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2. Ensure Admin User exists
        var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@gmail.com", cancellationToken);
        if (admin == null)
        {
            admin = new User
            {
                FullName = "System Admin",
                Email = "admin@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Ensure Packager User exists
        var packagerUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "packager@traveltour.com", cancellationToken);
        if (packagerUser == null)
        {
            packagerUser = new User
            {
                FullName = "Sunrise Travel Agent",
                Email = "packager@traveltour.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Packager@123"),
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(packagerUser);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 4. Ensure Packager profile exists
        var packager = await _context.Packagers.FirstOrDefaultAsync(p => p.UserId == packagerUser.Id, cancellationToken);
        if (packager == null)
        {
            packager = new Packager
            {
                UserId = packagerUser.Id,
                CompanyName = "Sunrise Travel Agency",
                BusinessLicenseNo = "LIC-9988223",
                ContactEmail = "packager@traveltour.com",
                ApprovedBy = admin.Id,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Packagers.Add(packager);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 5. Ensure Traveler User exists
        var traveler = await _context.Users.FirstOrDefaultAsync(u => u.Email == "traveler@traveltour.com", cancellationToken);
        if (traveler == null)
        {
            traveler = new User
            {
                FullName = "Alice Traveler",
                Email = "traveler@traveltour.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Traveler@123"),
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(traveler);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 6. Ensure Travel Package exists
        var hasPackage = await _context.Packages.AnyAsync(p => p.PackagerId == packager.Id, cancellationToken);
        if (!hasPackage)
        {
            var package = new Package
            {
                PackagerId = packager.Id,
                Title = "Scenic Maldives Tropical Escape",
                Description = "Experience a luxury speedboat tour, coral snorkeling, local island culture and 5-star beachfront resorts in this ultimate Maldives package.",
                Destination = "Male",
                Country = "Maldives",
                City = "Male",
                DurationDays = 5,
                DurationNights = 4,
                MaxCapacity = 40,
                CurrentBookings = 0,
                MinAge = 12,
                CancellationPolicy = "Free cancellation up to 14 days before travel.",
                IsFeatured = true,
                Type = PackageType.Group,
                Status = PackageStatus.Published,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Packages.Add(package);
            await _context.SaveChangesAsync(cancellationToken);

            // Add Seasonal Pricing
            var season = new PackageSeasonalPricing
            {
                PackageId = package.Id,
                SeasonName = "Peak Summer 2026",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                BasePrice = 50000.00m,
                ChildPrice = 30000.00m,
                DiscountPercent = 10.00m,
                AvailableSlots = 40,
                IsActive = true
            };
            _context.PackageSeasonalPricings.Add(season);

            // Add Highlights
            _context.PackageHighlights.Add(new PackageHighlight { PackageId = package.Id, HighlightText = "5-star beachfront stay" });
            _context.PackageHighlights.Add(new PackageHighlight { PackageId = package.Id, HighlightText = "Coral snorkeling & jet ski options" });
            _context.PackageHighlights.Add(new PackageHighlight { PackageId = package.Id, HighlightText = "Speedboat transfers included" });

            // Add Inclusions
            _context.PackageInclusions.Add(new PackageInclusion { PackageId = package.Id, Type = InclusionType.Included, Description = "Speedboat transfer" });
            _context.PackageInclusions.Add(new PackageInclusion { PackageId = package.Id, Type = InclusionType.Included, Description = "Daily Breakfast" });
            _context.PackageInclusions.Add(new PackageInclusion { PackageId = package.Id, Type = InclusionType.Optional, Description = "Water sports activities" });

            // Add Itinerary Days
            // Day 1
            var day1 = new ItineraryDay
            {
                PackageId = package.Id,
                DayNumber = 1,
                Title = "Arrival & Speedboat Transfer",
                Description = "Arrive at Male Airport and take a scenic speedboat transfer to the resort.",
                Location = "Male Airport",
                CreatedAt = DateTime.UtcNow
            };
            _context.ItineraryDays.Add(day1);
            await _context.SaveChangesAsync(cancellationToken);

            _context.ItineraryActivities.Add(new ItineraryActivity
            {
                ItineraryDayId = day1.Id,
                SequenceOrder = 1,
                ActivityTitle = "Welcome Island Orientation",
                ActivityType = "Sightseeing",
                Location = "Resort Beach",
                DurationMinutes = 45,
                IsOptional = false,
                ExtraCost = 0,
                DaySession = DaySession.Afternoon
            });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day1.Id, Venue = "Ocean View Grill", Description = "Welcome dinner set menu", IsIncluded = true, MealType = MealType.Dinner });
            _context.PackageAccommodations.Add(new PackageAccommodation { ItineraryDayId = day1.Id, HotelName = "Kurumba Maldives", HotelAddress = "Vihamanaafushi, Maldives", StarRating = 5, RoomType = "Superior beachfront room" });
            _context.PackageTransports.Add(new PackageTransport { ItineraryDayId = day1.Id, SegmentOrder = 1, VehicleDescription = "Resort Luxury Speedboat", PickupPoint = "Male Airport", DropPoint = "Kurumba Resort", TransportMode = TransportMode.Boat, DistanceKm = 10 });

            // Day 2
            var day2 = new ItineraryDay
            {
                PackageId = package.Id,
                DayNumber = 2,
                Title = "Coral Reef Snorkeling",
                Description = "Explore the vibrant underwater marine life of the Maldives coral reef.",
                Location = "Resort Reef",
                CreatedAt = DateTime.UtcNow
            };
            _context.ItineraryDays.Add(day2);
            await _context.SaveChangesAsync(cancellationToken);

            _context.ItineraryActivities.Add(new ItineraryActivity
            {
                ItineraryDayId = day2.Id,
                SequenceOrder = 1,
                ActivityTitle = "Guided House Reef Snorkeling",
                ActivityType = "Adventure",
                Location = "Marine Center",
                DurationMinutes = 90,
                IsOptional = false,
                ExtraCost = 0,
                DaySession = DaySession.Morning
            });
            _context.ItineraryActivities.Add(new ItineraryActivity
            {
                ItineraryDayId = day2.Id,
                SequenceOrder = 2,
                ActivityTitle = "Jet Skiing Safari",
                ActivityType = "Adventure",
                Location = "Water Sports Center",
                DurationMinutes = 30,
                IsOptional = true,
                ExtraCost = 2500,
                DaySession = DaySession.Afternoon
            });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day2.Id, Venue = "Resort Cafe", IsIncluded = true, MealType = MealType.Breakfast });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day2.Id, Venue = "Poolside Bar", IsIncluded = true, MealType = MealType.Lunch });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day2.Id, Venue = "Main Restaurant", IsIncluded = true, MealType = MealType.Dinner });
            _context.PackageAccommodations.Add(new PackageAccommodation { ItineraryDayId = day2.Id, HotelName = "Kurumba Maldives", HotelAddress = "Vihamanaafushi, Maldives", StarRating = 5, RoomType = "Superior beachfront room" });

            // Day 3
            var day3 = new ItineraryDay
            {
                PackageId = package.Id,
                DayNumber = 3,
                Title = "Himmafushi Village Guided Tour",
                Description = "Himmafushi guided local tour to experience Maldivian local culture.",
                Location = "Himmafushi",
                CreatedAt = DateTime.UtcNow
            };
            _context.ItineraryDays.Add(day3);
            await _context.SaveChangesAsync(cancellationToken);

            _context.ItineraryActivities.Add(new ItineraryActivity
            {
                ItineraryDayId = day3.Id,
                SequenceOrder = 1,
                ActivityTitle = "Village Local Market Guided Tour",
                ActivityType = "Sightseeing",
                Location = "Himmafushi Jetty",
                DurationMinutes = 120,
                IsOptional = false,
                ExtraCost = 0,
                DaySession = DaySession.Morning
            });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day3.Id, Venue = "Resort Cafe", IsIncluded = true, MealType = MealType.Breakfast });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day3.Id, Venue = "Local Cafe", Description = "Authentic Maldivian lunch", IsIncluded = true, MealType = MealType.Lunch });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day3.Id, Description = "Arrange dinner of choice at resort", IsIncluded = false, MealType = MealType.Dinner });
            _context.PackageAccommodations.Add(new PackageAccommodation { ItineraryDayId = day3.Id, HotelName = "Kurumba Maldives", HotelAddress = "Vihamanaafushi, Maldives", StarRating = 5, RoomType = "Superior beachfront room" });
            _context.PackageTransports.Add(new PackageTransport { ItineraryDayId = day3.Id, SegmentOrder = 1, VehicleDescription = "Local Ferry Boat", PickupPoint = "Kurumba Resort", DropPoint = "Himmafushi Island", TransportMode = TransportMode.Boat, DistanceKm = 15 });

            // Day 4
            var day4 = new ItineraryDay
            {
                PackageId = package.Id,
                DayNumber = 4,
                Title = "Spa Day & Sunset Cruise",
                Description = "A relaxing day of spa treatments followed by a cruise in the sunset.",
                Location = "Male Lagoon",
                CreatedAt = DateTime.UtcNow
            };
            _context.ItineraryDays.Add(day4);
            await _context.SaveChangesAsync(cancellationToken);

            _context.ItineraryActivities.Add(new ItineraryActivity
            {
                ItineraryDayId = day4.Id,
                SequenceOrder = 1,
                ActivityTitle = "Sunset Cruise with Dolphins",
                ActivityType = "Sightseeing",
                Location = "Male Lagoon",
                DurationMinutes = 120,
                IsOptional = false,
                ExtraCost = 0,
                DaySession = DaySession.Evening
            });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day4.Id, Venue = "Resort Cafe", IsIncluded = true, MealType = MealType.Breakfast });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day4.Id, Venue = "Beach Club", IsIncluded = true, MealType = MealType.Lunch });
            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day4.Id, Venue = "Hamakaze Teppanyaki", IsIncluded = true, MealType = MealType.Dinner });
            _context.PackageAccommodations.Add(new PackageAccommodation { ItineraryDayId = day4.Id, HotelName = "Kurumba Maldives", HotelAddress = "Vihamanaafushi, Maldives", StarRating = 5, RoomType = "Superior beachfront room" });

            // Day 5
            var day5 = new ItineraryDay
            {
                PackageId = package.Id,
                DayNumber = 5,
                Title = "Departure",
                Description = "Check out and return speedboat transfer to Male International Airport.",
                Location = "Male Airport",
                CreatedAt = DateTime.UtcNow
            };
            _context.ItineraryDays.Add(day5);
            await _context.SaveChangesAsync(cancellationToken);

            _context.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = day5.Id, Venue = "Resort Cafe", IsIncluded = true, MealType = MealType.Breakfast });
            _context.PackageTransports.Add(new PackageTransport { ItineraryDayId = day5.Id, SegmentOrder = 1, VehicleDescription = "Resort Speedboat", PickupPoint = "Kurumba Resort", DropPoint = "Male Airport", TransportMode = TransportMode.Boat, DistanceKm = 10 });

            await _context.SaveChangesAsync(cancellationToken);
        }

        // 7. Seed any user-supplied JSON packages (Package1.json, Package2.json)
        var packageFiles = new[] { "Package1.json", "Package2.json" };
        foreach (var file in packageFiles)
        {
            string? filePath = null;
            var current = Directory.GetCurrentDirectory();
            while (current != null)
            {
                var testPath = Path.Combine(current, file);
                if (System.IO.File.Exists(testPath))
                {
                    filePath = testPath;
                    break;
                }
                var parent = Directory.GetParent(current)?.FullName;
                if (parent != null)
                {
                    testPath = Path.Combine(parent, file);
                    if (System.IO.File.Exists(testPath))
                    {
                        filePath = testPath;
                        break;
                    }
                }
                current = parent;
            }

            if (filePath != null && System.IO.File.Exists(filePath))
            {
                try
                {
                    var jsonContent = await System.IO.File.ReadAllTextAsync(filePath, cancellationToken);
                    var packageData = System.Text.Json.JsonSerializer.Deserialize<CreatePackageRequest>(jsonContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (packageData != null)
                    {
                        var exists = await _context.Packages.AnyAsync(p => p.Title == packageData.Title && p.PackagerId == packager.Id, cancellationToken);
                        if (!exists)
                        {
                            var pkgType = Enum.TryParse<PackageType>(packageData.Type, true, out var tType) ? tType : PackageType.Group;
                            var pkgStatus = Enum.TryParse<PackageStatus>(packageData.Status, true, out var sStatus) ? sStatus : PackageStatus.Published;

                            var newPkg = new Package
                            {
                                PackagerId = packager.Id,
                                Title = packageData.Title,
                                Description = packageData.Description,
                                Destination = packageData.Destination,
                                Country = packageData.Country,
                                City = packageData.City,
                                DurationDays = packageData.DurationDays,
                                DurationNights = packageData.DurationNights,
                                MaxCapacity = packageData.MaxCapacity,
                                CurrentBookings = 0,
                                MinAge = packageData.MinAge ?? 0,
                                CancellationPolicy = packageData.CancellationPolicy,
                                IsFeatured = false,
                                Type = pkgType,
                                Status = pkgStatus,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.Packages.Add(newPkg);
                            await _context.SaveChangesAsync(cancellationToken);

                            // Highlights
                            if (packageData.Highlights != null)
                            {
                                foreach (var h in packageData.Highlights)
                                {
                                    _context.PackageHighlights.Add(new PackageHighlight
                                    {
                                        PackageId = newPkg.Id,
                                        HighlightText = h.HighlightText
                                    });
                                }
                            }

                            // Inclusions
                            if (packageData.Inclusions != null)
                            {
                                foreach (var inc in packageData.Inclusions)
                                {
                                    var incType = Enum.TryParse<InclusionType>(inc.InclusionType, true, out var iType) ? iType : InclusionType.Included;
                                    _context.PackageInclusions.Add(new PackageInclusion
                                    {
                                        PackageId = newPkg.Id,
                                        Type = incType,
                                        Description = inc.Description
                                    });
                                }
                            }

                            // Media
                            if (packageData.Media != null)
                            {
                                foreach (var m in packageData.Media)
                                {
                                    var mCat = Enum.TryParse<MediaCategory>(m.Category, true, out var cat) ? cat : MediaCategory.Destination;
                                    _context.PackageMedia.Add(new PackageMedium
                                    {
                                        PackageId = newPkg.Id,
                                        FilePath = m.FilePath ?? "",
                                        FileName = m.FileName,
                                        Caption = m.Caption,
                                        DisplayOrder = m.DisplayOrder,
                                        IsPrimary = m.IsPrimary,
                                        Category = mCat,
                                        MimeType = "image/jpeg",
                                        FileSizeBytes = 1024,
                                        UploadedAt = DateTime.UtcNow
                                    });
                                }
                            }

                            // Seasonal Pricing
                            if (packageData.SeasonalPricing != null)
                            {
                                foreach (var sp in packageData.SeasonalPricing)
                                {
                                    _context.PackageSeasonalPricings.Add(new PackageSeasonalPricing
                                    {
                                        PackageId = newPkg.Id,
                                        SeasonName = sp.SeasonName,
                                        StartDate = sp.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                                        EndDate = sp.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                                        BasePrice = sp.BasePrice,
                                        ChildPrice = sp.ChildPrice,
                                        DiscountPercent = sp.DiscountPercent,
                                        AvailableSlots = sp.AvailableSlots,
                                        IsActive = sp.IsActive
                                    });
                                }
                            }

                            // Itinerary Days
                            if (packageData.Itinerary != null)
                            {
                                foreach (var day in packageData.Itinerary)
                                {
                                    var newDay = new ItineraryDay
                                    {
                                        PackageId = newPkg.Id,
                                        DayNumber = day.DayNumber,
                                        Title = day.Title,
                                        Description = day.Description,
                                        Location = day.Location,
                                        CreatedAt = DateTime.UtcNow
                                    };

                                    _context.ItineraryDays.Add(newDay);
                                    await _context.SaveChangesAsync(cancellationToken);

                                    // Activities
                                    if (day.Activities != null)
                                    {
                                        foreach (var act in day.Activities)
                                        {
                                            var session = Enum.TryParse<DaySession>(act.DaySession, true, out var ds) ? ds : DaySession.Morning;
                                            _context.ItineraryActivities.Add(new ItineraryActivity
                                            {
                                                ItineraryDayId = newDay.Id,
                                                SequenceOrder = act.SequenceOrder,
                                                ActivityTitle = act.ActivityTitle,
                                                Description = act.Description,
                                                ActivityType = act.ActivityType ?? "Sightseeing",
                                                Location = act.Location,
                                                DurationMinutes = act.DurationMinutes ?? 60,
                                                IsOptional = act.IsOptional,
                                                ExtraCost = act.ExtraCost,
                                                DaySession = session
                                            });
                                        }
                                    }

                                    // Meals
                                    if (day.Meals != null)
                                    {
                                        foreach (var meal in day.Meals)
                                        {
                                            var mType = Enum.TryParse<MealType>(meal.MealType, true, out var mt) ? mt : MealType.Breakfast;
                                            _context.ItineraryDayMeals.Add(new ItineraryDayMeal
                                            {
                                                ItineraryDayId = newDay.Id,
                                                Venue = meal.Venue,
                                                Description = meal.Description,
                                                IsIncluded = meal.IsIncluded,
                                                MealType = mType
                                            });
                                        }
                                    }

                                    // Accommodations
                                    if (day.Accommodations != null)
                                    {
                                        foreach (var acc in day.Accommodations)
                                        {
                                            _context.PackageAccommodations.Add(new PackageAccommodation
                                            {
                                                ItineraryDayId = newDay.Id,
                                                HotelName = acc.HotelName,
                                                HotelAddress = acc.HotelAddress,
                                                StarRating = acc.StarRating ?? 3,
                                                RoomType = acc.RoomType,
                                                CheckInTime = TimeOnly.TryParse(acc.CheckInTime, out var cit) ? cit : null,
                                                CheckOutTime = TimeOnly.TryParse(acc.CheckOutTime, out var cot) ? cot : null,
                                                Amenities = acc.Amenities,
                                                Notes = acc.Notes
                                            });
                                        }
                                    }

                                    // Transports
                                    if (day.Transports != null)
                                    {
                                        foreach (var trans in day.Transports)
                                        {
                                            var tMode = Enum.TryParse<TransportMode>(trans.TransportMode, true, out var tm) ? tm : TransportMode.Bus;
                                            _context.PackageTransports.Add(new PackageTransport
                                            {
                                                ItineraryDayId = newDay.Id,
                                                SegmentOrder = trans.SegmentOrder,
                                                VehicleDescription = trans.VehicleDescription,
                                                PickupPoint = trans.PickupPoint,
                                                DropPoint = trans.DropPoint,
                                                PickupTime = TimeOnly.TryParse(trans.PickupTime, out var put) ? put : null,
                                                DropTime = TimeOnly.TryParse(trans.DropTime, out var dt) ? dt : null,
                                                DistanceKm = trans.DistanceKm ?? 0,
                                                Notes = trans.Notes,
                                                TransportMode = tMode
                                            });
                                        }
                                    }
                                }
                            }
                            
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error seeding package file {file}: {ex.Message}");
                }
            }
        }

        return Ok(new { success = true, message = "Dummy data seeded successfully! You can login with: admin@gmail.com / Admin@123, packager@traveltour.com / Packager@123, or traveler@traveltour.com / Traveler@123." });
    }
}
