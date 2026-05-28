file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.API/Controllers/PackagesController.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_endpoints = '''    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPackages([FromQuery] PackageSearchRequest request)
    {
        var result = await _packageService.SearchPackagesAsync(request);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPublishedPackages()
    {
        var packages = await _packageService.GetAllPublishedPackagesAsync();
        return Ok(packages);
    }'''

new_endpoints = '''    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPackages([FromQuery] PackageSearchRequest request)
    {
        var result = await _packageService.SearchPackagesAsync(request);
        return Ok(result);
    }'''

content = content.replace(old_endpoints, new_endpoints)

with open(file_path, 'w') as f:
    f.write(content)
