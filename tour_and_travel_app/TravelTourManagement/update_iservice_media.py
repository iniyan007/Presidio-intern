file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Interface/IPackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

if "using Microsoft.AspNetCore.Http;" not in content:
    content = content.replace("using TravelTourManagement.DataAccess.DTOs;", "using TravelTourManagement.DataAccess.DTOs;\nusing Microsoft.AspNetCore.Http;")

old_create = "Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, CancellationToken cancellationToken = default);"
new_create = "Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, List<IFormFile>? mediaFiles = null, CancellationToken cancellationToken = default);"

content = content.replace(old_create, new_create)

with open(file_path, 'w') as f:
    f.write(content)
