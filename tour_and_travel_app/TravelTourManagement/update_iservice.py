file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Interface/IPackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

if "using TravelTourManagement.DataAccess.DTOs;" not in content:
    content = content.replace("namespace TravelTourManagement.Business.Interface;", "namespace TravelTourManagement.Business.Interface;\nusing TravelTourManagement.DataAccess.DTOs;\nusing TravelTourManagement.DataAccess.DTOs.Packages;")

new_method = '''
    Task<IEnumerable<TravelTourManagement.DataAccess.DTOs.Packages.PackageSummaryResponse>> GetAllPublishedPackagesAsync(CancellationToken cancellationToken = default);

    Task<PagedResponse<TravelTourManagement.DataAccess.DTOs.Packages.PackageSummaryResponse>> SearchPackagesAsync(PackageSearchRequest request, CancellationToken cancellationToken = default);
'''

content = content.replace("Task<IEnumerable<TravelTourManagement.DataAccess.DTOs.Packages.PackageSummaryResponse>> GetAllPublishedPackagesAsync(CancellationToken cancellationToken = default);", new_method)

with open(file_path, 'w') as f:
    f.write(content)
