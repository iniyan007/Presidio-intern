import re
file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/PackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Fix StartDate and EndDate
content = content.replace("StartDate = p.StartDate,", "StartDate = p.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),")
content = content.replace("EndDate = p.EndDate,", "EndDate = p.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)),")

# Fix PackageSummaryResponse mapping
old_summary_mapping = '''        return new PackageSummaryResponse(
            package.Id,
            package.PackagerId,
            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Destination,
            package.Country,
            package.DurationDays,
            package.DurationNights,
            package.AvgRating,
            package.TotalReviews,
            primaryImage,
            startingPrice,
            pendingSeats
        );'''

new_summary_mapping = '''        return new PackageSummaryResponse(
            package.Id,
            package.PackagerId,
            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Type.ToString(),
            package.Destination,
            package.Country,
            package.DurationDays,
            package.DurationNights,
            package.AvgRating,
            package.TotalReviews,
            primaryImage,
            startingPrice,
            pendingSeats
        );'''
content = content.replace(old_summary_mapping, new_summary_mapping)

# Fix PackageDetailResponse mapping
old_detail_mapping = '''    private static PackageDetailResponse MapToPackageDetailResponse(Package package)
    {
        return new PackageDetailResponse(
            package.Id,
            package.PackagerId,
            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Description,
            package.Destination,
            package.Country,
            package.City,
            package.DurationDays,
            package.DurationNights,
            package.MaxCapacity,
            package.CurrentBookings,
            package.MinAge,
            package.CancellationPolicy,
            package.IsFeatured,
            package.AvgRating,
            package.TotalReviews,'''

new_detail_mapping = '''    private static PackageDetailResponse MapToPackageDetailResponse(Package package)
    {
        return new PackageDetailResponse(
            package.Id,
            package.PackagerId,
            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Description,
            package.Type.ToString(),
            package.Destination,
            package.Country,
            package.City,
            package.DurationDays,
            package.DurationNights,
            package.MaxCapacity,
            package.CurrentBookings,
            package.MinAge,
            package.CancellationPolicy,
            package.IsFeatured,
            package.AvgRating,
            package.TotalReviews,'''
content = content.replace(old_detail_mapping, new_detail_mapping)

with open(file_path, 'w') as f:
    f.write(content)
