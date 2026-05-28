file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/Repository/PackageRepository.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

if "using TravelTourManagement.DataAccess.DTOs.Packages;" not in content:
    content = content.replace("namespace TravelTourManagement.DataAccess.Repository;", "namespace TravelTourManagement.DataAccess.Repository;\nusing TravelTourManagement.DataAccess.DTOs.Packages;")

new_method = '''
    public async Task<(IReadOnlyList<Package> Packages, int TotalCount)> SearchPackagesAsync(
        PackageSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var lowerTerm = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(lowerTerm) ||
                p.Destination.ToLower().Contains(lowerTerm) ||
                p.Country.ToLower().Contains(lowerTerm) ||
                (p.City != null && p.City.ToLower().Contains(lowerTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            var lowerDest = request.Destination.ToLower();
            query = query.Where(p => p.Destination.ToLower().Contains(lowerDest));
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var lowerCountry = request.Country.ToLower();
            query = query.Where(p => p.Country.ToLower() == lowerCountry);
        }

        if (!string.IsNullOrWhiteSpace(request.PackageType))
        {
            if (Enum.TryParse<TravelTourManagement.DataAccess.Enums.PackageType>(request.PackageType, true, out var parsedType))
            {
                query = query.Where(p => p.Type == parsedType);
            }
        }

        if (request.MinDurationDays.HasValue)
            query = query.Where(p => p.DurationDays >= request.MinDurationDays.Value);

        if (request.MaxDurationDays.HasValue)
            query = query.Where(p => p.DurationDays <= request.MaxDurationDays.Value);

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.PackageSeasonalPricings.Any(sp => sp.IsActive &&
                (!request.MinPrice.HasValue || sp.BasePrice >= request.MinPrice.Value) &&
                (!request.MaxPrice.HasValue || sp.BasePrice <= request.MaxPrice.Value)));
        }

        // Apply Sorting
        query = request.SortBy?.ToLower() switch
        {
            "priceasc" => query.OrderBy(p => p.PackageSeasonalPricings.Where(sp => sp.IsActive).Min(sp => sp.BasePrice)),
            "pricedesc" => query.OrderByDescending(p => p.PackageSeasonalPricings.Where(sp => sp.IsActive).Min(sp => sp.BasePrice)),
            "ratingdesc" => query.OrderByDescending(p => p.AvgRating),
            "durationasc" => query.OrderBy(p => p.DurationDays),
            "durationdesc" => query.OrderByDescending(p => p.DurationDays),
            _ => query.OrderByDescending(p => p.CreatedAt) // Default is Newest
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var packages = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.PackageMedia)
            .Include(p => p.PackageSeasonalPricings)
            .Include(p => p.Packager)
            .ToListAsync(cancellationToken);

        return (packages, totalCount);
    }
'''

content = content.replace("public async Task AddPackageMediaAsync", new_method + "\n    public async Task AddPackageMediaAsync")

with open(file_path, 'w') as f:
    f.write(content)
