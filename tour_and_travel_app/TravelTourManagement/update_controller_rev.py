file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.API/Controllers/PackagesController.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

new_method = '''
    [HttpGet("{id}/revenue")]
    [Authorize(Roles = "Admin,Packager")]
    public async Task<IActionResult> GetPackageRevenue(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role))
            throw new UnauthorizedAccessException("Role not found in token.");

        var result = await _packageService.GetPackageRevenueAsync(userId, role, id);
        return Ok(result);
    }
}'''

last_brace_idx = content.rfind("}")
if last_brace_idx != -1:
    content = content[:last_brace_idx] + new_method

with open(file_path, 'w') as f:
    f.write(content)
