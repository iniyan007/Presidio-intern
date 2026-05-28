import re

# Update PackageSummaryResponse
file_path_summary = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/DTOs/Packages/PackageSummaryResponse.cs'
with open(file_path_summary, 'r') as f:
    content = f.read()

old_summary = '''    string PackagerName,
    string Title,'''
new_summary = '''    string PackagerName,
    string Title,
    string PackageType,'''
content = content.replace(old_summary, new_summary)

with open(file_path_summary, 'w') as f:
    f.write(content)


# Update PackageDetailResponse
file_path_detail = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.DataAccess/DTOs/Packages/PackageDetailResponse.cs'
with open(file_path_detail, 'r') as f:
    content = f.read()

old_detail = '''    string PackagerName,
    string Title,'''
new_detail = '''    string PackagerName,
    string Title,
    string PackageType,'''
content = content.replace(old_detail, new_detail)

with open(file_path_detail, 'w') as f:
    f.write(content)


# Update PackageService.cs
file_path_service = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/PackageService.cs'
with open(file_path_service, 'r') as f:
    content = f.read()

old_map_summary = '''            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Destination,'''
new_map_summary = '''            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Type.ToString(),
            package.Destination,'''
content = content.replace(old_map_summary, new_map_summary)

old_map_detail = '''            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Description,'''
new_map_detail = '''            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Type.ToString(),
            package.Description,'''
content = content.replace(old_map_detail, new_map_detail)

with open(file_path_service, 'w') as f:
    f.write(content)
