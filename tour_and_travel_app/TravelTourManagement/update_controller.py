file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.API/Controllers/PackagesController.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

new_method = '''
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPackages([FromQuery] PackageSearchRequest request)
    {
        var result = await _packageService.SearchPackagesAsync(request);
        return Ok(result);
    }
'''

content = content.replace("public async Task<IActionResult> GetAllPublishedPackages()", new_method + "\n    [HttpGet]\n    [AllowAnonymous]\n    public async Task<IActionResult> GetAllPublishedPackages()")

with open(file_path, 'w') as f:
    f.write(content)
