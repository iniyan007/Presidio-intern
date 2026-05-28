file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Interface/IPackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

new_method = "    Task<PackageRevenueResponse> GetPackageRevenueAsync(Guid userId, string role, Guid packageId, CancellationToken cancellationToken = default);\n}"

content = content.replace("}", new_method, 1)

with open(file_path, 'w') as f:
    f.write(content)
