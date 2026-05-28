file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/Interface/IPackageRepository.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

# Insert the namespace if not present
if "using TravelTourManagement.DataAccess.DTOs.Packages;" not in content:
    content = content.replace("namespace TravelTourManagement.DataAccess.Interface;", "namespace TravelTourManagement.DataAccess.Interface;\nusing TravelTourManagement.DataAccess.DTOs.Packages;")

new_method = '''
    Task<IReadOnlyList<Package>> GetAllPublishedWithFullDetailsAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Package> Packages, int TotalCount)> SearchPackagesAsync(PackageSearchRequest request, CancellationToken cancellationToken = default);
'''

content = content.replace("Task<IReadOnlyList<Package>> GetAllPublishedWithFullDetailsAsync(CancellationToken cancellationToken = default);", new_method)

with open(file_path, 'w') as f:
    f.write(content)
