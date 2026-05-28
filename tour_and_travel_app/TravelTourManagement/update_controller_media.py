file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.API/Controllers/PackagesController.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_create = '''    [HttpPost]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var packageId = await _packageService.CreatePackageAsync(userId, request);
        return CreatedAtAction(nameof(CreatePackage), new { id = packageId }, new { id = packageId, message = "Package created successfully." });
    }'''

new_create = '''    [HttpPost]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> CreatePackage([FromForm] CreatePackageCombinedRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        CreatePackageRequest packageData;
        try
        {
            packageData = System.Text.Json.JsonSerializer.Deserialize<CreatePackageRequest>(request.PackageData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            
            // Manual validation
            var context = new ValidationContext(packageData, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(packageData, context, results, true);
            if (!isValid)
            {
                return BadRequest(results);
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { message = "Invalid JSON in PackageData.", details = ex.Message });
        }

        var packageId = await _packageService.CreatePackageAsync(userId, packageData, request.MediaFiles);
        return CreatedAtAction(nameof(CreatePackage), new { id = packageId }, new { id = packageId, message = "Package created successfully." });
    }'''

content = content.replace(old_create, new_create)

old_upload = '''    [HttpPost("{id}/media")]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> UploadPackageMedia(Guid id, [FromForm] UploadPackageMediaRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        if (request.File == null || request.File.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = System.IO.Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!System.Linq.Enumerable.Contains(allowedExtensions, extension))
            throw new ArgumentException("Invalid file type. Only JPG, JPEG, and PNG are allowed.");

        if (request.File.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size exceeds 5MB limit.");

        using var stream = request.File.OpenReadStream();
        var fileName = await _packageService.UploadPackageMediaAsync(userId, id, stream, request.File.FileName, request.File.ContentType, request.Category, request.IsPrimary, request.DisplayOrder, request.Caption);
        return Ok(new { message = "Package media uploaded successfully.", fileName = fileName });
    }'''

content = content.replace(old_upload, "")

with open(file_path, 'w') as f:
    f.write(content)
