file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/PackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

if "using TravelTourManagement.DataAccess.DTOs;" not in content:
    content = content.replace("using TravelTourManagement.DataAccess.DTOs.Packages;", "using TravelTourManagement.DataAccess.DTOs.Packages;\nusing TravelTourManagement.DataAccess.DTOs;")

new_method = '''
    public async Task<PagedResponse<PackageSummaryResponse>> SearchPackagesAsync(PackageSearchRequest request, CancellationToken cancellationToken = default)
    {
        var (packages, totalCount) = await _packageRepository.SearchPackagesAsync(request, cancellationToken);
        var summaryResponses = packages.Select(MapToPackageSummaryResponse).ToList();
        return new PagedResponse<PackageSummaryResponse>(summaryResponses, totalCount, request.PageNumber, request.PageSize);
    }
'''

content = content.replace("public async Task<PackageDetailResponse> GetPublishedPackageByIdAsync", new_method + "\n    public async Task<PackageDetailResponse> GetPublishedPackageByIdAsync")

with open(file_path, 'w') as f:
    f.write(content)
