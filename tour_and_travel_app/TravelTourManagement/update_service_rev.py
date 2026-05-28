file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/PackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

new_method = '''
    public async Task<PackageRevenueResponse> GetPackageRevenueAsync(Guid userId, string role, Guid packageId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (role == "Packager")
        {
            var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (packager == null || package.PackagerId != packager.Id)
            {
                throw new UnauthorizedAccessException("You do not have permission to view revenue for this package.");
            }
        }

        var bookings = await _context.Bookings
            .Where(b => b.PackageId == packageId && 
                        (b.Status == TravelTourManagement.DataAccess.Enums.BookingStatus.Confirmed || b.Status == TravelTourManagement.DataAccess.Enums.BookingStatus.Completed))
            .ToListAsync(cancellationToken);

        decimal revenue = 0;
        string revenueType = "";

        if (role == "Admin")
        {
            revenue = bookings.Sum(b => b.PlatformFeeAmount);
            revenueType = "Platform Fee";
        }
        else if (role == "Packager")
        {
            revenue = bookings.Sum(b => b.PackagerBaseAmount);
            revenueType = "Packager Earnings";
        }

        return new PackageRevenueResponse
        {
            PackageId = packageId,
            PackageTitle = package.Title,
            Revenue = revenue,
            RevenueType = revenueType,
            TotalConfirmedBookings = bookings.Count
        };
    }
}'''

content = content.replace("}", new_method, 1) # wait, this is a bad idea for C#. 

with open(file_path, 'w') as f:
    f.write(content)
